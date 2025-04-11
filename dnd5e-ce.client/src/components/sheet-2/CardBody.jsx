import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';
import { useState } from 'react';

function CardBody({
  imgObj,
  controlId,
  labelText,
  cardStyles = "border-0",
  cardOverlayStyles = "p-2 pb-0",
  character,
  field,
  onHeaderColChange
}) {

  const [controlValue, setControlValue] = useState(character[field.sheet][field.name]);
  const handleChange = (event) => {
    const newValue = event.target.value;
    setControlValue(newValue);
    onHeaderColChange({ [field.name]: newValue });
  }

  return (
    <Card className={cardStyles}>
      <Card.Img src={imgObj} />
      <Card.ImgOverlay className={cardOverlayStyles}>
        <Form.Group controlId={controlId} className="d-flex flex-column h-100">
          <Form.Control
            as="textarea"
            value={controlValue}
            className="h-100"
            onChange={handleChange}
          />
          <div className="text-center">
            <Form.Label className="text-uppercase fw-bold">
              {labelText}
            </Form.Label>
          </div>
        </Form.Group>
      </Card.ImgOverlay>
    </Card>
  );
}

export default CardBody;