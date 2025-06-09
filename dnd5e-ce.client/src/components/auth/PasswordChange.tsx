import React, { useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router';
import { useAppDispatch, useAppSelector } from '../../hooks/index';
import { SubmitHandler, useForm } from "react-hook-form";
import { clearRedirectUrl, login, logout } from '../../store/slices/authSlice';

import api from '../../api/index';
import { IValidationError, IContainsAccessToken, IContainsTokens, IPasswordChangeData, AuthResponse } from '../../types/api';

import { Form, Button, Alert, Container, Row, Col, Spinner } from 'react-bootstrap';
import { toast } from 'react-toastify';
import { selectRedirectUrl } from '../../store/selectors/authSelectors';

const PasswordChange: React.FC = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const redirectUrl = useAppSelector(selectRedirectUrl);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    setError,
  } = useForm<IPasswordChangeData>({
    defaultValues: {
      oldPassword: '',
      newPassword: '',
      newPasswordConfirm: ''
    },
  });

  const onSubmit: SubmitHandler<IPasswordChangeData> = async (data) => {
    try 
    {
      const response = await api.post<AuthResponse>('/auth/change-password', {
        oldPassword: data.oldPassword,
        newPassword: data.newPassword,
        newPasswordConfirm: data.newPasswordConfirm
      });
      if (response.data.success)
      {
        dispatch(logout());
        toast.success("Пароль успешно изменён. Войдите в аккаунт снова");
        navigate("/login");
        dispatch(clearRedirectUrl());
      }
      else
      {
        throw new Error(response.data.errors?.join(", ") || "Password change failed.");
      }
    }
    catch (error: any)
    {
      if (axios.isAxiosError(error))
      {
        const errorData: AuthResponse = error.response?.data || {};
        const message = errorData.errors?.join(", ") || "Password change failed. Please check your credentials.";
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
          <h2>Смена пароля</h2>
          <Form onSubmit={handleSubmit(onSubmit)}>
            <Form.Group className="mb-3" controlId="oldPassword">
              <Form.Label>Текущий пароль</Form.Label>
              <Form.Control
                type="password"
                {...register('oldPassword', {
                  required: {
                    value: true,
                    message: 'Текущий пароль обязателен'
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
                isInvalid={!!errors.newPassword?.message}
              />
              <Form.Control.Feedback type="invalid">{errors.newPassword?.message}</Form.Control.Feedback>
            </Form.Group>
            <Form.Group className="mb-3" controlId="password">
              <Form.Label>Новый пароль</Form.Label>
              <Form.Control
                type="password"
                {...register('newPassword', {
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
                isInvalid={!!errors.newPassword?.message}
              />
              <Form.Control.Feedback type="invalid">{errors.newPassword?.message}</Form.Control.Feedback>
            </Form.Group>
            <Form.Group className="mb-3" controlId="newPasswordConfirm">
              <Form.Label>Подтверждение нового пароля</Form.Label>
              <Form.Control
                type="password"
                {...register('newPasswordConfirm',
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
                      passwordMatches: (newPasswordConfirm, { newPassword }) =>
                      {
                        if (newPasswordConfirm !== newPassword)
                        {
                          return 'Пароли не совпадают';
                        }
                      }
                    }
                  }
                )}
                isInvalid={!!errors.newPasswordConfirm?.message}
              />
              <Form.Control.Feedback type="invalid">{errors.newPasswordConfirm?.message}</Form.Control.Feedback>
            </Form.Group>
            <Button
              variant="primary"
              type="submit"
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <Spinner animation="border" role="status">
                  <span className="visually-hidden">Загрузка...</span>
                </Spinner>
              ) : ('Сменить пароль')}
            </Button>
          </Form>
        </Col>
      </Row>
    </Container>
  );
};

export default PasswordChange;