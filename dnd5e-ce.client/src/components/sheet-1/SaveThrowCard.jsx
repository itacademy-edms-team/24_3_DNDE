import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import './SaveThrowCard.css';

import { useState } from 'react';

function SaveThrowCard({
  character,
  savethrow,
  onSaveThrowProficiencyChange
}) {

  const [isProficient, setProficient] = useState(character.sheet1.saveThrows[savethrow.bondAbility].isProficient);
  const handleChange = (event) => {
    const newValue = event.target.checked;
    setProficient(newValue);
    onSaveThrowProficiencyChange(savethrow.bondAbility, { isProficient: newValue });
  }

  const countPb = (level) => {
    if ([1, 2, 3, 4].includes(level)) return 2;
    if ([5, 6, 7, 8].includes(level)) return 3;
    if ([9, 10, 11, 12].includes(level)) return 4;
    if ([13, 14, 15, 16].includes(level)) return 5;
    if ([17, 18, 19, 20].includes(level)) return 6;
  }

  const renderAncCalcResult = () => {
    const bondAbility = savethrow.bondAbility;
    const abilityBase = character.sheet1.characteristics[bondAbility].base;
    const abilityModifier = Math.floor((abilityBase - 10) / 2);

    const pb = isProficient ? countPb(character.sheet1.level) : 0;
    //console.log(pb);

    const other = 0;
    return abilityModifier + pb + other;
  }
  
  return (
    // text-truncate позволяет нормально ресайзить текст внутри блоков. Не удалять
    <Col className="d-flex flex-row align-items-center border-0 text-truncate"> 
      <Col md={"auto"} className="d-flex flex-column align-items-center justify-content-center p-0 m-0">
        <Form.Group controlId={savethrow.checkControlId} className="p-0 pt-0 pb-0 m-0">
          <Form.Check
            checked={isProficient}
            className="p-0 m-0 label-bonus" 
            onChange={handleChange} />
        </Form.Group>
      </Col>
      <Col md={2} className="d-flex justify-content-center align-items-center">
        <Form.Group controlId={savethrow.resultControlId }>
          <Form.Control readOnly value={renderAncCalcResult()} className="border-0 bg-transparent shadow-none p-0 m-0 text-center label-bonus" />
        </Form.Group>
      </Col>
      <Col md={"auto"} className="d-flex justify-content-center align-items-center">
        <Form.Group controlId={savethrow.labelControlId} className="text-center text-truncate w-100">
          <Form.Label className="p-0 m-0 label-bonus">{savethrow.labelText}</Form.Label>
        </Form.Group>
      </Col>
    </Col>
  );
}

export default SaveThrowCard;