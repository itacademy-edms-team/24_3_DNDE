import React from 'react';
import { BrowserRouter as Router, Routes, Route, NavLink } from 'react-router';
import { Navbar, Nav, Container, Button } from 'react-bootstrap';
import { useNavigate } from 'react-router';
import { useAppSelector, useAppDispatch } from './hooks/index';
import { selectAccessToken } from './store/selectors/authSelectors';
import Register from './components/auth/Register';
import Login from './components/auth/Login';
import SheetSelect from './components/sheet-selection/SheetSelect';
import Dashboard from './components/Dashboard';
import AuthRequired from './components/auth/AuthRequired';
import LogoutButton from './components/auth/LogoutButton';

const App: React.FC = () => {
  const dispatch = useAppDispatch();

  const token = useAppSelector(selectAccessToken);

  return (
    <Router>
      <Navbar bg="dark" variant="dark" expand="lg">
        <Container>
          <Navbar.Brand as={NavLink} to="/">DND5E-CE</Navbar.Brand>
          <Navbar.Toggle aria-controls="basic-navbar-nav" />
          <Navbar.Collapse id="basic-navbar-nav">
            <Nav className="me-auto">
              <Nav.Link as={NavLink} to="/" end>Главная</Nav.Link>
              {!token ? (
                <>
                  <Nav.Link as={NavLink} to="/register">Регистрация</Nav.Link>
                  <Nav.Link as={NavLink} to="/login">Вход</Nav.Link>
                </>
              ) : (
                <>
                  <Nav.Link as={NavLink} to="/sheet-selection">Персонажи</Nav.Link>
                  <Nav.Link as={NavLink} to="/dashboard">Выбранный персонаж</Nav.Link>
                </>
              )}
            </Nav>
            {token && (
              <Nav className="ms-auto">
                <LogoutButton />
              </Nav>
            )}
          </Navbar.Collapse>
        </Container>
      </Navbar>
      <Routes>
        <Route path="/" element={<Container className="mt-5"><h1>Добро пожаловать в Редактор персонажа</h1></Container>} />
        <Route path="/register" element={<Register />} />
        <Route path="/login" element={<Login />} />
        <Route path="/sheet-selection" element={<AuthRequired> <SheetSelect /> </AuthRequired>} />
        <Route path="/dashboard" element={<AuthRequired> <Dashboard /> </AuthRequired>} />
      </Routes>
    </Router>
  );
}

export default App;