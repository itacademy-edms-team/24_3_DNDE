import React, { Fragment } from 'react';
import { Alert } from 'react-bootstrap';
import { useAppSelector } from '../../hooks/index';
import { selectAccessToken } from '../../store/selectors/authSelectors';

interface AuthRequiredProps {
  children: React.ReactNode;
}

const AuthRequired: React.FC<AuthRequiredProps> = ({ children }) => {
  const token = useAppSelector(selectAccessToken);

  return <>{token ? children : <Alert variant="danger">Please log in to view this content.</Alert>}</>;
};

export default AuthRequired;