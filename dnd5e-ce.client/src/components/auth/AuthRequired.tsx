import React, { Fragment, useEffect, useState } from 'react';
import { Alert } from 'react-bootstrap';
import { useAppDispatch, useAppSelector } from '../../hooks/index';
import { selectIsUserAuthenticated } from '../../store/selectors/authSelectors';

import api from '../../api/index';
import { loading, login, logout } from '../../store/slices/authSlice';
import { useNavigate } from 'react-router';
import axios from 'axios';

interface AuthRequiredProps {
  children: React.ReactNode;
}

const AuthRequired: React.FC<AuthRequiredProps> = ({ children }) =>
{
  const dispatch = useAppDispatch();

  const navigate = useNavigate();

  useEffect(() => 
  {
    const checkAuth = async () => 
    {
      try 
      {
        dispatch(loading());
        console.log("Checking authorization...");
        await api.get("/auth/check-auth");
        dispatch(login());
        console.log("Authorization is active");
      }
      catch (error) 
      {
        if (axios.isAxiosError(error) && error.response?.status === 401) 
        {
          console.log("Unauthorized access - 401.");
          try
          {
            console.log("Trying refresh tokens...");
            await api.post("/auth/refresh-token");
            dispatch(login());
            console.log("Tokens refreshed");
          }
          catch (error) 
          {
            if (axios.isAxiosError(error) && error.response?.status === 401)
            {
              console.log("Tokens expired. Need relogin");
              dispatch(logout());
              navigate("/login");
            }
            else
            {
              console.log(`Unexpected error: ${error}`);
            }
          }
        }
        else
        {
          console.log(`Unexpected error: ${error}`);
        }
      }
    };

    checkAuth();
  }, []);

  const isAuthenticated = useAppSelector(selectIsUserAuthenticated);

  // Пока проверка не завершена, показываем индикатор загрузки
  if (isAuthenticated === null) {
    return <div>Loading...</div>;
  }

  // Если авторизован, показываем children, иначе - Alert
  return (
    <>
      {isAuthenticated ? (
        children
      ) : (
        <Alert variant="danger">Please log in to view this content.</Alert>
      )}
    </>
  );
};

export default AuthRequired;