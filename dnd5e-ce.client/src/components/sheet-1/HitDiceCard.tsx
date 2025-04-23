import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import HitDiceSVG from './assets/HitDice.svg';

import { useState } from 'react';
import { useAppDispatch, useAppSelector } from '../../hooks';
import { updateHitDiceTotal, updateHitDiceCurrent, updateHitDiceType } from '../../store/sheet1Slice';
import { RootState, HitDiceType } from '../../types/state';

const HitDiceCard: React.FC = () => {
  const dispatch = useAppDispatch();

  const diceTotal = useAppSelector((state: RootState) => state.sheet1.hitDice.total);
  const diceCurrent = useAppSelector((state: RootState) => state.sheet1.hitDice.current);
  const diceType = useAppSelector((state: RootState) => state.sheet1.hitDice.type);

  const handleDiceTotal = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(e.target.value);
    if (!isNaN(newValue)) {
      dispatch(updateHitDiceTotal(newValue));
    }
  };

  const handleDiceCurrent = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(e.target.value);
    if (!isNaN(newValue)) {
      dispatch(updateHitDiceCurrent(newValue));
    }
  };

  const handleDiceType = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const newValue = e.target.value as HitDiceType;  
    dispatch(updateHitDiceType(newValue));
  };

  return (
    <Card className="border-0 p-0 m-0">
      <Card.Img src={HitDiceSVG} />
      <Card.ImgOverlay className="p-0 m-0 d-flex flex-column justify-content-between text-center text-truncate h-100">
        <Form.Group
          controlId="characterHitDiceTotal"
          className="d-flex flex-row"
        >
          <Form.Label
            className="p-2 pt-1 pb-0 m-0 fw-lighter"
          >
            Итого
          </Form.Label>
          <Form.Control
            type="number"
            value={diceTotal}
            className="p-2 pt-1 pb-0 m-0 bg-transparent border-0 border-bottom shadow-none text-center"
            onChange={handleDiceTotal}
          />

        </Form.Group>
        <Form.Group
          controlId="characterHitDiceCurrent"
          className="flex-grow-1 d-flex flex-row"
        >
          <Form.Control
            type="number"
            value={diceCurrent}
            className="p-0 pt-0 pb-0 m-0 bg-transparent border-0 shadow-none text-center"
            onChange={handleDiceCurrent}
          />
        </Form.Group>
        <Form.Group
          controlId="characterHitDiceType"
          className="d-flex flex-row pb-1"
        >
          <Form.Label
            className="p-2 pt-0 pb-0 m-0 text-uppercase fw-bold"
            style={{fontSize: "0.8rem"}}
          >
            Кость хитов
          </Form.Label>
          <Form.Select
            value={diceType}
            size="sm"
            className="p-0 m-0 border-0 shadow-none bg-transparent"
            onChange={handleDiceType}
          >
            <option value="D4">D4</option>
            <option value="D6">D6</option>
            <option value="D8">D8</option>
            <option value="D10">D10</option>
            <option value="D12">D12</option>
          </Form.Select>
        </Form.Group>
      </Card.ImgOverlay>
    </Card>
  );
}

export default HitDiceCard;