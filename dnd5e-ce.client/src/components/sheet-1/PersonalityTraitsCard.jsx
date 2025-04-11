import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import { useState } from 'react';

import PersonalityTraitsSVG from './assets/PersonalityTraits.svg';

function PersonalityTraitsCard({
  character,
  onCharacterPersonalityTraitsChange
}) {

  const [personalityTraits, setPersonalityTraits] = useState(character.sheet1.personalityTraits);

  const handleChange = (event) => {
    const newValue = event.target.value;
    setPersonalityTraits(newValue);
    onCharacterPersonalityTraitsChange({ personalityTraits: newValue });
  };

  //console.log(character.sheet1.personalityTraits);

  return (
    <Card className="border-0 p-0 m-0">
      <Card.Img src={PersonalityTraitsSVG} />
      <Card.ImgOverlay className="p-2 pt-0 pb-1 m-0">
        <Form.Group
          controlId="characterPersonalyTraits"
          className="d-flex flex-column justify-content-between text-center text-truncate h-100"
        >
          <Form.Control
            as="textarea"
            type="text"
            value={personalityTraits}
            className="p-1 m-0 bg-transparent border-0 shadow-none flex-grow-1"
            onChange={handleChange}
          />
          <Form.Label
            className="p-0 pt-0 pb-0 m-0 text-uppercase fw-bold"
          >
            Черты характера
          </Form.Label>
        </Form.Group>
      </Card.ImgOverlay>
    </Card>
  );
}

export default PersonalityTraitsCard;