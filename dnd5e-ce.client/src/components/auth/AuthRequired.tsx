import React, { Fragment, useEffect, useState } from 'react';
import { Alert } from 'react-bootstrap';
import { useAppSelector } from '../../hooks/index';
import { selectAccessToken } from '../../store/selectors/authSelectors';

import api from '../../api/index';

interface AuthRequiredProps {
  children: React.ReactNode;
}

const AuthRequired: React.FC<AuthRequiredProps> = ({ children }) => {
  const [isAuthenticated, setIsAuthenticated] = useState<boolean | null>(null);

  useEffect(() => {
    const checkAuth = async () => {
      try {
        await api.get('/auth/me'); // Запрос к эндпоинту для проверки авторизации
        setIsAuthenticated(true);
      } catch (error) {
        console.error('Auth check failed:', error);
        setIsAuthenticated(false);
      }
    };

    checkAuth();
  }, []);

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