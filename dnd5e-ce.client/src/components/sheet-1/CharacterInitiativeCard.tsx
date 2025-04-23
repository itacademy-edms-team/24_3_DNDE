import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import InitiativeSVG from './assets/Initiative.svg';

import { useState } from 'react';

import { useAppDispatch, useAppSelector } from '../../hooks';
import { updateInitiative } from '../../store/sheet1Slice';
import { RootState } from '../../types/state';

const CharacterInitiativeCard: React.FC = () => {
  const dispatch = useAppDispatch();

  const initiative = useAppSelector((state: RootState) => state.sheet1.initiative);
  const handleInitiative = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newInitiative = parseInt(e.target.value, 10);
    if (!isNaN(newInitiative)) {
      dispatch(updateInitiative(newInitiative));
    }
  }

  return (
    <Col>
      <Card className="border-0 p-0 m-0">
        <Card.Img src={InitiativeSVG} />
        <Card.ImgOverlay className="p-2 pt-0 pb-1 m-0">
          <Form.Group
            controlId="characterInitiative"
            className="d-flex flex-column justify-content-between text-center text-truncate h-100"
          >
            <Form.Control
              type="number"
              value={initiative}
              className="flex-grow-1 p-0 m-0 bg-transparent border-0 shadow-none text-center"
              onChange={handleInitiative}
            />
            <Form.Label
              className="p-0 pt-0 pb-0 m-0 text-uppercase fw-bold"
              style={{ fontSize: "0.7rem" }}
            >
              Инициатива
            </Form.Label>
          </Form.Group>
        </Card.ImgOverlay>
      </Card>
    </Col>
    
  );
}

export default CharacterInitiativeCard;