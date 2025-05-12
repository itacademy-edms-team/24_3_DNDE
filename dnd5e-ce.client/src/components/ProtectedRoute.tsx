import React from 'react';
import { Route, Navigate } from 'react-router';
import { useAppSelector } from '../hooks/index';
import { selectAccessToken } from '../store/selectors/authSelectors';

interface ProtectedRouteProps {
  component: React.ComponentType;
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ component: Component, ...rest }) => {
  const token = useAppSelector(selectAccessToken);

  return (
    <Route
      {...rest}
      element={token ? <Component /> : <Navigate to="/login" replace />}
    />
  );
};

export default ProtectedRoute;