import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import { useState } from 'react';

import FlawsSVG from './assets/Flaws.svg';

function CharacterFlaws({
  character,
  onCharacterFlawsChange
}) {

  const [flaws, setFlaws] = useState(character.sheet1.flaws);

  const handleChange = (event) => {
    const newValue = event.target.value;
    setFlaws(newValue);
    onCharacterFlawsChange({ flaws: newValue });
  };

  //console.log(character.sheet1.flaws);

  return (
    <Card className="border-0 p-0 m-0">
      <Card.Img src={FlawsSVG} />
      <Card.ImgOverlay className="p-2 pt-0 pb-1 m-0">
        <Form.Group
          controlId="characterFlaws"
          className="d-flex flex-column justify-content-between text-center text-truncate h-100"
        >
          <Form.Control
            as="textarea"
            type="text"
            value={flaws}
            className="p-1 m-0 bg-transparent border-0 shadow-none flex-grow-1"
            onChange={handleChange}
          />
          <Form.Label
            className="p-0 pt-0 pb-0 m-0 text-uppercase fw-bold"
          >
            Слабости
          </Form.Label>
        </Form.Group>
      </Card.ImgOverlay>
    </Card>
  );
}

export default CharacterFlaws;