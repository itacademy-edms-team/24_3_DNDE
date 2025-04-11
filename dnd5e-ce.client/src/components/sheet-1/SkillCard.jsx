import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';
import { useState } from 'react';

function SkillCard({
  character,
  skill,
  onSkillProficiencyChange
}) {

  const [isProficient, setProficient] = useState(character.sheet1.skills[skill.fieldName].isProficient);
  const handleProficientChange = (event) => {
    const newValue = event.target.checked;
    setProficient(newValue);
    onSkillProficiencyChange(skill.fieldName, { isProficient: newValue });
  }

  const countPb = (level) => {
    if ([1, 2, 3, 4].includes(level)) return 2;
    if ([5, 6, 7, 8].includes(level)) return 3;
    if ([9, 10, 11, 12].includes(level)) return 4;
    if ([13, 14, 15, 16].includes(level)) return 5;
    if ([17, 18, 19, 20].includes(level)) return 6;
  }

  const renderAndCountValue = () => {
    const bondAbility = skill.bondAbility;
    const abilityBase = character.sheet1.characteristics[bondAbility].base
    const abilityModifier = Math.floor((abilityBase - 10) / 2);

    const pb = isProficient ? countPb(character.sheet1.level) : 0;
    //console.log(pb);

    const other = 0;
    return abilityModifier + pb + other;
  }

  //console.log(character.skills);

  return (
    <Col className="d-flex flex-row align-items-center border-0 text-truncate">
      <Col md={"auto"} className="d-flex flex-column justify-content-center align-items-center">
        <Form.Group controlId={skill.checkControlId} className="p-0 pt-0 pb-0 m-0">
          <Form.Check
            checked={isProficient}
            className="p-0 m-0 label-bonus"
            onChange={handleProficientChange}
          />
        </Form.Group>
      </Col>
      <Col md={3} className="d-flex justify-content-center align-items-center">
        <Form.Group controlId={skill.resultControlId}>
          <Form.Control readOnly value={renderAndCountValue()} className="border-0 bg-transparent shadow-none p-0 m-0 text-center label-bonus" />
        </Form.Group>
      </Col>
      <Col md={"auto"} className="d-flex justify-content-center align-items-center">
        <Form.Group controlId={skill.labelControlId} className="text-center text-truncate w-100">
          <Form.Label className="p-0 m-0 label-bonus">{skill.labelText}</Form.Label>
        </Form.Group>
      </Col>
    </Col>
  );
}

export default SkillCard;