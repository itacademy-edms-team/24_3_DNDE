import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import { useAppSelector, useAppDispatch } from '../hooks/index';
import {  } from '../store/selectors/authSelectors';
import { Container, Row, Col, Button, Card } from 'react-bootstrap';

const Home: React.FC = () => {
  
  return (
    <Container className="mt-3">
      <h1>Добро пожаловать в Редактор персонажа</h1>
    </Container>
  );
}

export default Home;