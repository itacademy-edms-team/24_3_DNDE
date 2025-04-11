import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import PassivePerceptionSVG1 from './assets/PassivePerceptionCircle.svg';
import PassivePerceptionSVG2 from './assets/PassivePerceptionText.svg';

function PassivePerceptionCard({
  character
}) {

  const countPb = (level) => {
    if ([1, 2, 3, 4].includes(level)) return 2;
    if ([5, 6, 7, 8].includes(level)) return 3;
    if ([9, 10, 11, 12].includes(level)) return 4;
    if ([13, 14, 15, 16].includes(level)) return 5;
    if ([17, 18, 19, 20].includes(level)) return 6;
    return 0;
  }

  const renderAncCalcResult = () => {
    const abilityMod = Math.floor((character.sheet1.characteristics.wisdom.base - 10) / 2);
    const hasProficiencyInPerception = character.sheet1.skills.perception.isProficient;
    const pb = hasProficiencyInPerception ? countPb(character.sheet1.level) : 0;
    return 10 + abilityMod + pb;
  }

  return (
    <Card className="d-flex flex-row justify-content-center gap-0 border-0">
      <Card className="d-flex flex-column justify-content-center border-0">
        <Card.Img src={PassivePerceptionSVG1} alt="Card Image" />
        <Card.ImgOverlay className="d-flex flex-column justify-content-center align-items-center p-0 m-0">
          <Form.Group controlId="characterProficiencyBonus" className="d-flex align-items-center justify-content-center w-100 h-100">
            <Form.Control readOnly value={renderAncCalcResult()} className="border-0 bg-transparent shadow-none text-center" />
          </Form.Group>
        </Card.ImgOverlay>
      </Card>
      <Card className="d-flex flex-column justify-content-center border-0">
        <Card.Img src={PassivePerceptionSVG2} />
        <Card.ImgOverlay className="d-flex flex-column justify-content-center p-0 m-0">
          <Form.Group controlId="characterProficiencyBonus" className="text-center text-truncate">
            <Form.Label className="text-uppercase fw-bold p-0 m-0 label-bonus">Пассивное восприятие</Form.Label>
          </Form.Group>
        </Card.ImgOverlay>
      </Card>
    </Card>
  );
}

export default PassivePerceptionCard;