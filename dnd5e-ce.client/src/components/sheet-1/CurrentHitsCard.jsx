import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';
import { useState } from 'react';


import CurrentHitsSVG from './assets/CurrentHits.svg';

function CurrentHitsCard({
  character,
  onCharacterMaxHPChange,
  onCharacterCurrentHPChange
}) {

  const [maxHP, setMaxHP] = useState(character.sheet1.hp.max);
  const [currentHP, setCurrentHP] = useState(character.sheet1.hp.current);

  const handleMaxHPChange = (event) => {
    const newValue = event.target.value;
    setMaxHP(newValue);
    onCharacterMaxHPChange({ max: newValue });
  };

  const handleCurrentHPChange = (event) => {
    const newValue = event.target.value;
    setCurrentHP(newValue);
    onCharacterCurrentHPChange({ current: newValue });
  };

  //console.log(character);

  return (
    <Card className="border-0 p-0 m-0">
      <Card.Img src={CurrentHitsSVG} />
      <Card.ImgOverlay className="p-0 m-0 d-flex flex-column justify-content-between text-center text-truncate h-100">
        <Form.Group
          controlId="characterMaxHP"
          className="d-flex flex-row"
        >
          <Form.Label
            className="p-2 pt-1 pb-0 m-0 text-uppercase fw-lighter"
          >
            Максимум хитов
          </Form.Label>
          <Form.Control
            type="text"
            value={maxHP}
            className="p-2 pt-1 pb-0 m-0 bg-transparent border-0 border-bottom shadow-none text-center"
            onChange={handleMaxHPChange}
          />

        </Form.Group>
        <Form.Group
          controlId="characterCurrentHP"
          className="p-0 m-0 flex-grow-1 d-flex flex-column justify-content-between text-center text-truncate"
        >
          <Form.Control
            type="number"
            value={currentHP}
            className="p-0 m-0 bg-transparent border-0 shadow-none text-center flex-grow-1"
            onChange={handleCurrentHPChange}
          />
          <Form.Label
            className="p-0 pb-1 m-0 text-uppercase fw-bold"
          >
            Текущие хиты
          </Form.Label>
        </Form.Group>
      </Card.ImgOverlay>
    </Card>
  );
}

export default CurrentHitsCard;