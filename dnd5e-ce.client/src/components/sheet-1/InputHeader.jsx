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
  isReadOnly = false,
  character,
  attrName,
  onInputHeaderUpdate
}) {

  const [val, setVal] = useState(character.sheet1[attrName]);

  const handleChange = (event) => {
    const newValue = event.target.value;
    setVal(newValue);
    onInputHeaderUpdate({ [attrName]: newValue })
  }

  //console.log(character);

  return (
    <Form.Group controlId={controlId}>
      <Form.Control
        type={controlType}
        value={val}
        readOnly={isReadOnly}
        placeholder={controlPlaceholder}
        onChange={handleChange}
      />
      <Form.Label className="fw-bold text-uppercase">{controlLabel}</Form.Label>
    </Form.Group>
  );
}

export default InputHeader;