import React from 'react';
import { useNavigate } from 'react-router';
import { useAppSelector, useAppDispatch } from '../../hooks/index';
import { selectAccessToken } from '../../store/selectors/authSelectors';

import { Container, Row, Col, Button, Card } from 'react-bootstrap';
import { FaTrash, FaPlus } from 'react-icons/fa';

const SheetSelect: React.FC = () => {
  return (
    <Container className="mt-5">
      <h2>Выбор персонажа</h2>
      <Row className="row-cols-md-8 d-flex flex-row justify-content-center">
        <Card className="w-100">
          <Card.Body
            className="d-flex gap-3 justify-content-between align-items-center"
          >
            <div className="flex-grow-1" style={{ cursor: "pointer" }}>
              <Card.Title>Новый персонаж</Card.Title>
              <Card.Title>Уровень: 1</Card.Title>
            </div>
            <FaTrash
              style={{ minWidth: "2rem", minHeight: "2rem", color: "red", cursor: "pointer" }}
            />
          </Card.Body>
        </Card>
      </Row>
      <Container className="mt-2 d-flex justify-content-center">
        <FaPlus
          style={{ minWidth: "1.5rem", minHeight: "1.5rem", cursor: "pointer", }}
        />
      </Container>
    </Container>
  );
}

export default SheetSelect;