import { Button } from 'react-bootstrap';
import { useNavigate } from 'react-router';
import { useAppSelector, useAppDispatch } from '../../hooks/index';
import { logout } from '../../store/slices/authSlice';

const LogoutButton: React.FC = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const handleLogout = () => {
    dispatch(logout());
  }

  return (
    <Button variant="danger" onClick={() => {
      dispatch(logout());
      navigate('/login');
    }}>
      Выйти из аккаунта
    </Button>
  );
}

export default LogoutButton;