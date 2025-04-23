import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';
import { useState } from 'react';

import TempHitsSVG from './assets/TemporaryHits.svg';

import { useAppDispatch, useAppSelector } from '../../hooks';
import { updateTempHP } from '../../store/sheet1Slice';
import { RootState } from '../../types/state';

const TempHitsCard: React.FC = () => {
  const dispatch = useAppDispatch();

  const tempHP = useAppSelector((state: RootState) => state.sheet1.hp.temp);
  const handleTempHPChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(e.target.value);
    if (!isNaN(newValue)) {
      dispatch(updateTempHP(newValue));
    }
  };
  

  return (
    <Card className="border-0 p-0 m-0">
      <Card.Img src={TempHitsSVG} />
      <Card.ImgOverlay className="p-2 m-0 d-flex flex-column justify-content-between text-center text-truncate h-100">
        <Form.Group
          controlId="characterTemporaryHP"
          className="p-0 m-0 flex-grow-1 d-flex flex-column justify-content-between text-center text-truncate"
        >
          <Form.Control
            type="number"
            value={tempHP}
            className="p-0 m-0 bg-transparent border-0 shadow-none text-center flex-grow-1"
            onChange={handleTempHPChange}
          />
          <Form.Label
            className="p-0 pb-1 m-0 text-uppercase fw-bold"
          >
            Временные хиты
          </Form.Label>
        </Form.Group>
      </Card.ImgOverlay>
    </Card>
  );
}

export default TempHitsCard;