import React, { useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router';
import { useAppDispatch, useAppSelector } from '../../hooks/index';
import { SubmitHandler, useForm } from "react-hook-form";
import { clearRedirectUrl, login } from '../../store/slices/authSlice';

import api from '../../api/index';
import { IValidationError, IContainsAccessToken, IContainsTokens, ILoginFormData, AuthResponse } from '../../types/api';

import { Form, Button, Alert, Container, Row, Col, Spinner } from 'react-bootstrap';
import { toast } from 'react-toastify';
import { selectRedirectUrl } from '../../store/selectors/authSelectors';

interface ILoginFormData {
  email: string;
  password: string;
}

const Login: React.FC = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const redirectUrl = useAppSelector(selectRedirectUrl);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    setError,
  } = useForm<ILoginFormData>({
    defaultValues: {
      email: '',
      password: ''
    },
  });

  const onSubmit: SubmitHandler<ILoginFormData> = async (data) => {
    try 
    {
      const response = await api.post<AuthResponse>('/auth/login', {
        email: data.email,
        password: data.password,
      });
      if (response.data.success)
      {
        dispatch(login());
        toast.success("Logged in successfully!");
        navigate(redirectUrl || "/sheet-selection");
        dispatch(clearRedirectUrl());
      }
      else
      {
        throw new Error(response.data.errors?.join(", ") || "Login failed.");
      }
    }
    catch (error: any)
    {
      if (axios.isAxiosError(error))
      {
        const errorData: AuthResponse = error.response?.data || {};
        const message = errorData.errors?.join(", ") || "Login failed. Please check your credentials.";
        toast.error(message);
        }
      else
      {
        console.error("Unexpected error:", error);
        toast.error("An unexpected error occurred.");
      }
    }
  }

  return (
    <Container>
      <Row className="justify-content-md-center mt-5">
        <Col md={6}>
          <h2>Вход</h2>
          <Form onSubmit={handleSubmit(onSubmit)}>
            <Form.Group className="mb-3" controlId="email">
              <Form.Label>Почта</Form.Label>
              <Form.Control
                type="email"
                {...register('email', {
                  required: {
                    value: true,
                    message: 'Почта обязательна'
                  },
                  pattern: {
                    value: /^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$/,
                    message: 'Почта не соответствует формату'
                  }
                })}
                isInvalid={!!errors.email?.message}
              />
              <Form.Control.Feedback type="invalid">{errors.email?.message}</Form.Control.Feedback>
            </Form.Group>
            <Form.Group className="mb-3" controlId="password">
              <Form.Label>Пароль</Form.Label>
              <Form.Control
                type="password"
                {...register('password', {
                  required: {
                    value: true,
                    message: 'Пароль обязателен'
                  },
                  minLength: {
                    value: 8,
                    message: 'Пароль должен быть длиннее 8 символов'
                  },
                  maxLength: {
                    value: 20,
                    message: 'Пароль должен быть короче 20 символов'
                  }
                })}
                isInvalid={!!errors.password?.message}
              />
              <Form.Control.Feedback type="invalid">{errors.password?.message}</Form.Control.Feedback>
            </Form.Group>
            <Button
              variant="primary"
              type="submit"
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <Spinner animation="border" role="status">
                  <span className="visually-hidden">Loading...</span>
                </Spinner>
              ) : ('Войти')}
            </Button>
          </Form>
        </Col>
      </Row>
    </Container>
  );
};

export default Login;