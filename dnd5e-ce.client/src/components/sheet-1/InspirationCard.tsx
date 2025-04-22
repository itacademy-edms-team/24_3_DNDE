import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';


import InspirationSVG1 from './assets/InspirationSquare.png';
import InspirationSVG2 from './assets/InspirationText.png';

import { useState } from 'react';
import { useAppDispatch, useAppSelector } from '../../hooks';
import { RootState } from '../../types/state';

import { updateInspiration } from '../../store/sheet1Slice';

const InspirationCard: React.FC = () => {
  const dispatch = useAppDispatch();

  const isInspired = useAppSelector((state: RootState) => state.sheet1.isInspired);
  const handleIsInspiredChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    dispatch(updateInspiration(e.target.checked));
  }

  return (
    <Container className="d-flex flex-row justify-content-center gap-0 border-0">
      <Card className="d-flex flex-column justify-content-center border-0">
        <Card.Img src={InspirationSVG1} alt="Card Image" />
        <Card.ImgOverlay className="d-flex flex-column justify-content-center align-items-center p-0 m-0">
          <Form.Group controlId="characterInspiration" className="d-flex align-items-center justify-content-center w-100 h-100">
            <Form.Check
              checked={isInspired}
              onChange={handleIsInspiredChange}
            />
          </Form.Group>
        </Card.ImgOverlay>
      </Card>
      <Card className="d-flex flex-column justify-content-center border-0">
        <Card.Img src={InspirationSVG2} />
        <Card.ImgOverlay className="d-flex flex-column justify-content-center p-0 m-0">
          <Form.Group controlId="characterInspiration" className="text-center">
            <Form.Label className="text-uppercase fw-bold p-0 m-0">Вдохновение</Form.Label>
          </Form.Group>
        </Card.ImgOverlay>
      </Card>
    </Container>
  );
}

export default InspirationCard;