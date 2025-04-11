import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Form from 'react-bootstrap/Form';
import { useState } from 'react';

function InputHeader({
  controlId,
  controlType = "text",
  controlPlaceholder,
  controlLabel,
  character,
  field,
  onFieldChange
}) {

  const [controlValue, setControlValue] = useState(character[field.sheet][field.name]);
  const handleChange = (event) => {
    const newValue = event.target.value;
    setControlValue(newValue);
    onFieldChange({ [field.name]: newValue }, field.sheet);
  }

  return (
    <Form.Group controlId={controlId}>
      <Form.Control
        type={controlType}
        value={controlValue}
        placeholder={controlPlaceholder}
        onChange={handleChange}
      />
      <Form.Label className="fw-bold text-uppercase">{controlLabel}</Form.Label>
    </Form.Group>
  );
}

export default InputHeader;