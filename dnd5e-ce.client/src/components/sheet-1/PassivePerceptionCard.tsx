import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import PassivePerceptionSVG1 from './assets/PassivePerceptionCircle.svg';
import PassivePerceptionSVG2 from './assets/PassivePerceptionText.svg';

import { useAppSelector } from '../../hooks';
import { selectPassivePerception } from '../../store/selectors/sheet1Selectors';

function PassivePerceptionCard() {
  const calculatedResult = useAppSelector(selectPassivePerception);

  return (
    <Card className="d-flex flex-row justify-content-center gap-0 border-0">
      <Card className="d-flex flex-column justify-content-center border-0">
        <Card.Img src={PassivePerceptionSVG1} alt="Card Image" />
        <Card.ImgOverlay className="d-flex flex-column justify-content-center align-items-center p-0 m-0">
          <Form.Group controlId="characterProficiencyBonus" className="d-flex align-items-center justify-content-center w-100 h-100">
            <Form.Control
              value={calculatedResult}
              readOnly
              className="border-0 bg-transparent shadow-none text-center" />
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