import { Button } from 'react-bootstrap';
import { useNavigate } from 'react-router';
import { useAppSelector, useAppDispatch } from '../../hooks/index';
import { logout } from '../../store/slices/authSlice';
import api from '../../api';
import axios from 'axios';
import { toast } from 'react-toastify';
import { AuthResponse } from '../../types/api';

const LogoutButton: React.FC = () =>
{
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const handleLogout = async () =>
  {
    try
    {
      const response = await api.post<AuthResponse>('/auth/revoke-token');
      if (response.data.success)
      {
        dispatch(logout());
        toast.success('Logged out successfully!');
        navigate('/login');
      } else
      {
        throw new Error(response.data.errors?.join(', ') || 'Logout failed.');
      }
    } catch (error: any)
    {
      if (axios.isAxiosError(error))
      {
        const errorData: AuthResponse = error.response?.data || {};
        const message = errorData.errors?.join(', ') || 'Logout failed. Please try again.';
        toast.error(message);
      } else
      {
        console.error('Unexpected error:', error);
        toast.error('An unexpected error occurred.');
      }
    }
  };

  return (
    <Button variant="danger" onClick={handleLogout}>
      Выйти из аккаунта
    </Button>
  );
};

export default LogoutButton;
