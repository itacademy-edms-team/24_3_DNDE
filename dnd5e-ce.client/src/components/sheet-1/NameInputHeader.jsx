import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Form from 'react-bootstrap/Form';

function NameInputHeader({
  controlId,
  controlType = "text",
  controlPlaceholder,
  controlLabel,
  characterName,
  onCharacterNameChange
}) {

  const handleChange = (event) => {
    const newValue = event.target.value;
    onCharacterNameChange({ name: newValue });
  }

  return (
    <Form.Group controlId={controlId}>
      <Form.Control
        type={controlType}
        value={characterName}
        placeholder={controlPlaceholder}
        onChange={handleChange}
      />
      <Form.Label className="fw-bold text-uppercase">{controlLabel}</Form.Label>
    </Form.Group>
  );
}

export default NameInputHeader;