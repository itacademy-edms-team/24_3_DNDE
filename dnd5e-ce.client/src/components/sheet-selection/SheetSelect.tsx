import React from 'react';
import { useNavigate } from 'react-router';
import { useAppSelector, useAppDispatch } from '../../hooks/index';
import { selectAccessToken } from '../../store/selectors/authSelectors';

import { Container, Row, Col, Button, Card } from 'react-bootstrap';

const SheetSelect: React.FC = () => {
  const token = useAppSelector(selectAccessToken);
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  return (
    <Container className="mt-5">
      <h2>Выбор персонажа</h2>
    </Container>
  );
}

export default SheetSelect;