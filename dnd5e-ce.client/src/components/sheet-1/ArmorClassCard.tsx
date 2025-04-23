import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import ArmorClassSVG from './assets/ArmorClass.svg';

import { useState } from 'react';

import { useAppDispatch, useAppSelector } from '../../hooks';
import { updateArmorClass } from '../../store/sheet1Slice';
import { RootState } from '../../types/state';


const ArmorClassCard: React.FC = () => {
  const dispatch = useAppDispatch();
  const ac = useAppSelector((state: RootState) => state.sheet1.armorClass);

  const handleACChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(e.target.value);
    if (!isNaN(newValue)) {
      dispatch(updateArmorClass(newValue));
    }
  };

  return (
    <Card className="border-0 p-0 m-0">
      <Card.Img src={ArmorClassSVG} />
      <Card.ImgOverlay className="p-0 m-0">
        <Form.Group
          controlId="characterInitiative"
          className="d-flex flex-column justify-content-between text-center text-truncate h-100"
        >
          <Form.Control
            type="number"
            value={ac}
            className="flex-grow-1 p-0 pt-lg-3 m-0 bg-transparent border-0 shadow-none text-center"
            onChange={handleACChange}
          />
          <Form.Label
            className="p-0 pt-0 pb-3 m-0 text-uppercase fw-bold"
          >
            КЗ
          </Form.Label>
        </Form.Group>
      </Card.ImgOverlay>
    </Card>
  );
}

export default ArmorClassCard;