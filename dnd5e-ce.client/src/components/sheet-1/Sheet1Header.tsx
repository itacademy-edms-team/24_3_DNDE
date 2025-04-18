import React from 'react';
import { useAppDispatch, useAppSelector } from '../../hooks';
import { updateName } from '../../store/sheet1Slice';

import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Form from 'react-bootstrap/Form';

const Sheet1Header: React.FC = () => {
  const dispatch = useAppDispatch();
  const name = useAppSelector((state) => state.sheet1.name);
  const handleNameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    dispatch(updateName(e.target.value));
  }

  return (
    <Container className="mt-3">
      <Row>
        <Col md={3} className="d-flex flex-column align-items-center justify-content-center">
          <Form.Group>
            <Form.Control
              id="characterName"
              value={name}
              onChange={handleNameChange}
            />
            <Form.Label className="text-uppercase fw-bold">Имя персонажа</Form.Label>
          </Form.Group>
        </Col>
        <Col md={1} />
        <Col md={8}>
          <Row>
            <Col md={2}>
              1
            </Col>
            <Col md={2}>
              2
            </Col>
            <Col md={4}>
              3
            </Col>
            <Col md={4}>
              4
            </Col>
            <Col md={4}>
              5
            </Col>
            <Col md={4}>
              6
            </Col>
            <Col md={4}>
              7
            </Col>
          </Row>
        </Col>
      </Row>
    </Container>
  );
};

export default Sheet1Header;