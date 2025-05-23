import { Button } from 'react-bootstrap';
import { useNavigate } from 'react-router';
import { useAppSelector, useAppDispatch } from '../../hooks/index';
import { logout } from '../../store/slices/authSlice';

const LogoutButton: React.FC = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const handleLogout = () => {
    dispatch(logout());
    navigate("/login");
  }

  return (
    <Button variant="danger" onClick={handleLogout}>
      Выйти из аккаунта
    </Button>
  );
}

export default LogoutButton;