import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';


import InspirationSVG1 from './assets/InspirationSquare.png';
import InspirationSVG2 from './assets/InspirationText.png';
import { useState } from 'react';

function InspirationCard({
  character,
  onInspirationChange
}) {

  const [isInspired, setInspiration] = useState(character.sheet1.isInspired);
  const handleChange = (event) => {
    const newValue = event.target.checked;
    setInspiration(newValue);
    onInspirationChange({ isInspired: newValue });
  }

  return (
    <Card className="d-flex flex-row justify-content-center gap-0 border-0">
      <Card className="d-flex flex-column justify-content-center border-0">
        <Card.Img src={InspirationSVG1} alt="Card Image" />
        <Card.ImgOverlay className="d-flex flex-column justify-content-center align-items-center p-0 m-0">
          <Form.Group controlId="characterInspiration" className="d-flex align-items-center justify-content-center w-100 h-100">
            <Form.Check
              checked={isInspired}
              onChange={handleChange}
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
    </Card>
  );
}

export default InspirationCard;