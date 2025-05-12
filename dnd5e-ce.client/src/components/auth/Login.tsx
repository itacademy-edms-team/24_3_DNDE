import React, { useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router';
import { useAppDispatch } from '../../hooks/index';
import { SubmitHandler, useForm } from "react-hook-form";
import { login } from '../../store/slices/authSlice';

import api from '../../api/index';
import { IValidationError, IContainsAccessToken, IContainsTokens } from '../../types/api';

import { Form, Button, Alert, Container, Row, Col } from 'react-bootstrap';

interface ILoginFormData {
  email: string;
  password: string;
}

const Login: React.FC = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

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
    try {
      const response = await api.post<IContainsTokens>('/api/auth/login', {
        email: data.email,
        password: data.password,
      });

      dispatch(login({ accessToken: response.data.accessToken, refreshToken: response.data.refreshToken }));
      navigate('/sheet-selection');
    } catch (error: any) {
      console.log(error);
      const errorData = error.response?.data || [];
      errorData.forEach((err: IValidationError) => {
        if (err.code.includes('Email')) {
          setError('email', { type: 'server', message: err.description });
        } else if (err.code.includes('Password')) {
          setError('password', { type: 'server', message: err.description });
        }
      });
      if (!errorData.length) {
        setError('root', { type: 'server', message: errorData });
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
              {isSubmitting ? 'Входим...' : 'Войти'}
            </Button>
          </Form>
        </Col>
      </Row>
    </Container>
  );
};

export default Login;