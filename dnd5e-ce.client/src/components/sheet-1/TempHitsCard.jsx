import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';
import { useState } from 'react';


import TempHitsSVG from './assets/TemporaryHits.svg';

function TempHitsCard({
  character,
  onCharacterTempHPChange
}) {

  const [tempHP, setTempHP] = useState(character.sheet1.hp.temp);

  const handleTempHPChange = (event) => {
    const newValue = event.target.value;
    setTempHP(newValue);
    onCharacterTempHPChange({ temp: newValue });
  };

  //console.log(character);

  return (
    <Card className="border-0 p-0 m-0">
      <Card.Img src={TempHitsSVG} />
      <Card.ImgOverlay className="p-0 m-0 d-flex flex-column justify-content-between text-center text-truncate h-100">
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