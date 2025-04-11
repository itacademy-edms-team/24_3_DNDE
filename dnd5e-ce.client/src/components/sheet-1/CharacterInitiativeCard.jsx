import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import { useState } from 'react';

import InitiativeSVG from './assets/Initiative.svg';

function CharacterInitiativeCard({
  character,
  onCharacterInitiativeChange
}) {

  const [initiative, setInitiative] = useState(character.sheet1.initiative);

  const handleChange = (event) => {
    const newValue = event.target.value;
    setInitiative(newValue);
    onCharacterInitiativeChange({ initiative: newValue });
  };

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
              className="p-0 m-0 bg-transparent border-0 shadow-none text-center flex-grow-1"
              onChange={handleChange}
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