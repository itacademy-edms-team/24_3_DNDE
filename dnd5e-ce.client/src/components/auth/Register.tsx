import axios, { AxiosResponse } from 'axios';
import React, { useState } from 'react';
import { useAppDispatch } from '../../hooks/index';
import { login } from '../../store/slices/authSlice';
import { useNavigate } from 'react-router';
import { SubmitHandler, useForm } from 'react-hook-form';

import api from '../../api/index';

import { Form, Button, Alert, Container, Row, Col, Spinner } from 'react-bootstrap';

import { AuthResponse, IRegisterFormData, IValidationError } from '../../types/api';
import { toast } from 'react-toastify';

const Register: React.FC = () => {
  const navigate = useNavigate();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    setError,
  } = useForm<IRegisterFormData>({
    defaultValues: {
      username: '',
      email: '',
      password: ''
    },
  });

  const onSubmit: SubmitHandler<IRegisterFormData> = async (data) => {
    try
    {
      const response: AxiosResponse<AuthResponse> = await api.post('/auth/register', {
        username: data.username,
        email: data.email,
        password: data.password,
      });
      navigate('/login');
    }
    catch (error: any)
    {
      if (axios.isAxiosError(error))
      {
        const errorData: AuthResponse = error.response?.data || {};
        const message = errorData.errors?.join(", ") || "Register failed. Please check your credentials.";
        toast.error(message);
        }
      else
      {
        console.error("Unexpected error:", error);
        toast.error("An unexpected error occurred.");
      }
    }
  };

  return (
    <Container>
      <Row className="justify-content-md-center mt-5">
        <Col md={6}>
          <h2>Регистрация</h2>
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
            <Form.Group className="mb-3" controlId="username">
              <Form.Label>Имя пользователя</Form.Label>
              <Form.Control
                type="text"
                {...register('username', {
                  required: {
                    value: true,
                    message: 'Имя пользователя обязательно'
                  },
                  minLength: {
                    value: 5,
                    message: 'Имя пользователя должно быть длиннее 5 символов'
                  },
                  maxLength: {
                    value: 20,
                    message: 'Имя пользователя должно быть короче 20 символов'
                  }
                })}
                isInvalid={!!errors.username?.message}
              />
              <Form.Control.Feedback type="invalid">{errors.username?.message}</Form.Control.Feedback>
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
            <Form.Group className="mb-3" controlId="passwordConfirm">
              <Form.Label>Подтверждение пароля</Form.Label>
              <Form.Control
                type="password"
                {...register('passwordConfirm',
                  {
                    required: {
                      value: true,
                      message: "Подтверждение пароля обязательно"
                    },
                    minLength: {
                      value: 8,
                      message: 'Подтверждение пароля должно быть длиннее 8 символов'
                    },
                    maxLength: {
                      value: 20,
                      message: 'Подтверждение пароля должно быть короче 20 символов'
                    },
                    validate: {
                      passwordMatches: (passwordConfirm, { password }) => {
                        if (passwordConfirm !== password) {
                          return 'Пароли не совпадают';
                        }
                      }
                    }
                  }
                )}
                isInvalid={!!errors.passwordConfirm?.message}
              />
              <Form.Control.Feedback type="invalid">{errors.passwordConfirm?.message}</Form.Control.Feedback>
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
              ): ('Регистрация')}
            </Button>
          </Form>
        </Col>
      </Row>
    </Container>
  );
};

export default Register;