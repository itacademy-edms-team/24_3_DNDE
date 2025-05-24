import React, { useEffect, useState } from 'react';
import { BrowserRouter as Router, Routes, Route, NavLink } from 'react-router';
import { Navbar, Nav, Container, Button } from 'react-bootstrap';

import { useAppSelector, useAppDispatch } from './hooks/index';
import { selectIsUserAuthenticated } from './store/selectors/authSelectors';
import { login, logout } from './store/slices/authSlice';
import api from './api/index';

import Register from './components/auth/Register';
import Login from './components/auth/Login';
import SheetSelect from './components/sheet-selection/SheetSelect';
import Dashboard from './components/Dashboard';
import AuthRequired from './components/auth/AuthRequired';
import LogoutButton from './components/auth/LogoutButton';
import axios from 'axios';
import Home from './components/Home';
import { ToastContainer } from 'react-toastify';


const App: React.FC = () => {
  const isSignedIn = useAppSelector(selectIsUserAuthenticated);

  return (
    <Router>
      <Navbar bg="dark" variant="dark" expand="lg">
        <Container>
          <Navbar.Brand as={NavLink} to="/">DND5E-CE</Navbar.Brand>
          <Navbar.Toggle aria-controls="basic-navbar-nav" />
          <Navbar.Collapse id="basic-navbar-nav">
            <Nav className="me-auto">
              <Nav.Link as={NavLink} to="/" end>Главная</Nav.Link>
              {!isSignedIn ? (
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
            {isSignedIn && (
              <Nav className="ms-auto">
                <LogoutButton />
              </Nav>
            )}
          </Navbar.Collapse>
        </Container>
      </Navbar>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/register" element={<Register />} />
        <Route path="/login" element={<Login />} />
        <Route path="/sheet-selection" element={<AuthRequired> <SheetSelect /> </AuthRequired>} />
        <Route path="/dashboard" element={<AuthRequired> <Dashboard /> </AuthRequired>} />
      </Routes>
      <ToastContainer />
    </Router>
  );
}

export default App;