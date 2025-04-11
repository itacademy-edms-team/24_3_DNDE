import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';
import { useState } from 'react';

import HitDiceSVG from './assets/HitDice.svg';

function DeathSaveThrowsCard({
  character,
  onDeathSaveThrowSuccessesChange,
  onDeathSaveThrowFailuresChange
}) {

  const [successes, setSuccesses] = useState(character.sheet1.deathSaveThrow.successes);
  const [failures, setFailures] = useState(character.sheet1.deathSaveThrow.failures);

  const handleSuccessChange = (index, newValue) => {
    let newSuccesses = [false, false, false];
    if (newValue) {
      // Если устанавливаем галочку
      if (index === 0) newSuccesses = [true, false, false];
      if (index === 1) newSuccesses = [true, true, false];
      if (index === 2) newSuccesses = [true, true, true];
    } else {
      // Если снимаем галочку
      if (index === 0) newSuccesses = [false, false, false];
      if (index === 1) newSuccesses = [true, false, false];
      if (index === 2) newSuccesses = [true, true, false];
    }
    setSuccesses(newSuccesses);
    onDeathSaveThrowSuccessesChange({ successes: newSuccesses })
  };

  const handleFailureChange = (index, newValue) => {
    if (newValue) {
      if (index === 0) setFailures([true, false, false]);
      if (index === 1) setFailures([true, true, false]);
      if (index === 2) setFailures([true, true, true]);
    } else {
      if (index === 0) setFailures([false, false, false]);
      if (index === 1) setFailures([true, false, false]);
      if (index === 2) setFailures([true, true, false]);
    }
  };

  const renderCheckboxGroup = (title, groupId, group, handleChange) => (
    <Card className="p-0 px-1 d-flex flex-row border-0 bg-transparent" style={{ flexWrap: 'wrap' }}>
      <Form.Label className="p-0 px-0 pt-0 pb-0 m-0 fw-lighter" style={{ fontSize: '0.8rem' }}>
        {title}
      </Form.Label>
      {group.map((checked, index) => (
        <Form.Check
          key={`${groupId}${index}`} // characterDeathThrowSucces1, characterDeathThrowSucces2, ...
          inline
          className="p-0 m-0"
          style={{ transform: 'scale(0.8)', margin: '0.1rem' }}
          checked={checked}
          onChange={(e) => handleChange(index, e.target.checked)}
        />
      ))}
    </Card>
  );

  //console.log(character);

  return (
    <Card className="border-0 p-0 m-0">
      <Card.Img src={HitDiceSVG} />
      <Card.ImgOverlay className="p-0 pt-1 px-2 m-0">
        {renderCheckboxGroup("Успехи", "characterDeathThrowSucces", successes, handleSuccessChange)}
        {renderCheckboxGroup("Провалы", "characterDeathThrowFail", failures, handleFailureChange)}
        <Form.Label className="p-0 m-0 text-center text-uppercase fw-bold" style={{fontSize: "0.8rem"} }>Спасброски от смерти</Form.Label>
      </Card.ImgOverlay>
    </Card>
  );
}

export default DeathSaveThrowsCard;