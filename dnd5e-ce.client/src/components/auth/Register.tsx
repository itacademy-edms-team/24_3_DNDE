import axios from 'axios';
import React, { useState } from 'react';
import { useAppDispatch } from '../../hooks/index';
import { login } from '../../store/slices/authSlice';
import { useNavigate } from 'react-router';

import api from '../../api/index';

import { Form, Button, Alert, Container, Row, Col } from 'react-bootstrap';
import { SubmitHandler, useForm } from 'react-hook-form';

import { IValidationError } from '../../types/api';

interface IRegisterFormData {
  username: string;
  email: string;
  password: string;
  passwordConfirm: string;
}

const Register: React.FC = () => {
  const navigate = useNavigate();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    setError,
  } = useForm<IRegisterFormData>({
    defaultValues: {
      email: '',
      password: ''
    },
  });

  const onSubmit: SubmitHandler<IRegisterFormData> = async (data) => {
    try {
      const response = await api.post('/api/auth/register', {
        username: data.username,
        email: data.email,
        password: data.password,
      });
      navigate('/login');
    } catch (error: any) {
      console.log(error);
      const errorData = error.response?.data || [];
      errorData.forEach((err: IValidationError) => {
        if (err.code.includes('UserName')) {
          setError('username', { type: 'server', message: err.description });
        } else if (err.code.includes('Email')) {
          setError('email', { type: 'server', message: err.description });
        } else if (err.code.includes('Password')) {
          setError('password', { type: 'server', message: err.description });
        }
      });
      if (!errorData.length) {
        setError('root', { type: 'server', message: errorData });
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
                    required: true,
                    minLength: 8,
                    maxLength: 20,
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
              {isSubmitting ? 'Регистрируемся...' : 'Регистрация'}
            </Button>
          </Form>
        </Col>
      </Row>
    </Container>
  );
};

export default Register;