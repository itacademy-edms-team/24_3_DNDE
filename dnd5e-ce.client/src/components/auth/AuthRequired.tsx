import React, { Fragment, useEffect, useState } from 'react';
import { Alert, Button, Container, Spinner } from 'react-bootstrap';
import { useAppDispatch, useAppSelector } from '../../hooks/index';
import { selectHasChecked, selectIsUserAuthenticated } from '../../store/selectors/authSelectors';

import api from '../../api/index';
import { loading, login, logout, setRedirectUrl } from '../../store/slices/authSlice';
import { useNavigate } from 'react-router';
import axios, { AxiosResponse } from 'axios';
import { useLocation } from 'react-router';
import { toast } from 'react-toastify';
import { AuthResponse } from '../../types/api';

interface AuthRequiredProps {
  children: React.ReactNode;
}

const AuthRequired: React.FC<AuthRequiredProps> = ({ children }) =>
{
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const location = useLocation();

  const isAuthenticated = useAppSelector(selectIsUserAuthenticated);
  const hasChecked = useAppSelector(selectHasChecked);

  useEffect(() =>
  {
    const checkAuth = async () =>
    {
      // Skip if already authenticated or checked
      if (isAuthenticated === true || hasChecked)
      {
        return;
      }

      const controller = new AbortController();
      try
      {
        dispatch(loading());
        console.log("Checking authorization...");
        const response: AxiosResponse<string> = await api.get("/auth/check-auth", {
          signal: controller.signal
        });
        dispatch(login());
        console.log("Authorization is active");
      }
      catch (error)
      {
        if (axios.isAxiosError(error))
        {
          if (error.response?.status === 401)
          {
            console.log("Unauthorized access - 401.");
            try
            {
              console.log("Trying refresh tokens...");
              let attempts = 2;
              let refreshError: any;
              while (attempts > 0)
              {
                try
                {
                  const refreshResponse: AxiosResponse<AuthResponse> = await api.post(
                    "/auth/refresh-token",
                    null,
                    { signal: controller.signal }
                  );
                  if (refreshResponse.data.success)
                  {
                    dispatch(login());
                    console.log("Tokens refreshed");
                    return; // Success, no further action
                  } else
                  {
                    throw new Error(refreshResponse.data.errors?.join(", ") || "Token refresh failed");
                  }
                }
                catch (err)
                {
                  refreshError = err;
                  attempts--;
                }
              }
              console.log("Tokens expired. Need relogin");
              dispatch(logout());
              dispatch(setRedirectUrl(location.pathname + location.search));
              toast.error("Session expired. Please log in.");
              window.location.href = "/login"; // Avoid navigate to prevent loops
            }
            catch (refreshError)
            {
              console.error("Refresh token error:", refreshError);
              dispatch(logout());
              dispatch(setRedirectUrl(location.pathname + location.search));
              toast.error("Failed to refresh session. Please log in.");
              window.location.href = "/login";
            }
          } else
          {
            console.error("Unexpected error:", error);
            toast.error(error.response?.data.errors?.join(", ") || "Failed to verify authentication");
            dispatch(logout());
            dispatch(setRedirectUrl(location.pathname + location.search));
            window.location.href = "/login";
          }
        } else
        {
          console.error("Unexpected error:", error);
          toast.error("An unexpected error occurred during authentication");
          dispatch(logout());
          dispatch(setRedirectUrl(location.pathname + location.search));
          window.location.href = "/login";
        }
      }

      return () =>
      {
        controller.abort();
      };
    };

    checkAuth();
  }, [dispatch, navigate, location, isAuthenticated, hasChecked]); // TODO: May cause lag. Re-Check dependency later

  if (isAuthenticated === null || !hasChecked)
  {
    return (
      <Container className="d-flex justify-content-center align-items-center" style={{ minHeight: "50vh" }}>
        <Spinner animation="border" role="status">
          <span className="visually-hidden">Loading...</span>
        </Spinner>
      </Container>
    );
  }

  // Если авторизован, показываем children, иначе - Alert
  return isAuthenticated ? (
    <>{children}</>
  ) : (
    <Container className="mt-5">
      <Alert variant="danger" className="d-flex flex-column align-items-center">
        <Alert.Heading>Войдите, чтобы увидеть содержимое.</Alert.Heading>
        <Button
          variant="primary"
          onClick={() =>
          {
            dispatch(setRedirectUrl(location.pathname + location.search));
            navigate("/login");
          }}
        >
          Войти
        </Button>
      </Alert>
    </Container>
  );
};


export default AuthRequired;