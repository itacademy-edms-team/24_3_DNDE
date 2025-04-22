import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import ProficiencyBonusSVG1 from './assets/ProficiencyBonusCircle.png';
import ProficiencyBonusSVG2 from './assets/ProficiencyBonusText.png';

import { useAppSelector } from '../../hooks';
import { RootState } from '../../types/state';

import './ProficiencyBonusCard.css';
import { useMemo } from 'react';

const ProficiencyBonusCard: React.FC = () => {
  const calculatePB = (level: number) => {
    if (level >= 1 && level <= 4) return 2;
    if (level >= 5 && level <= 8) return 3;
    if (level >= 9 && level <= 12) return 4;
    if (level >= 13 && level <= 16) return 5;
    if (level >= 17 && level <= 20) return 6;
    return 0;
  }

  const chLevel = useAppSelector((state: RootState) => state.sheet1.level);

  const proficiencyBonus = useMemo(() => {
    return calculatePB(chLevel);
  }, [chLevel]);

  return (
    <Card className="d-flex flex-row justify-content-center gap-0 border-0">
      <Card className="d-flex flex-column justify-content-center border-0">
        <Card.Img src={ProficiencyBonusSVG1} alt="Card Image" />
        <Card.ImgOverlay className="d-flex flex-column justify-content-center align-items-center p-0 m-0">
          <Form.Group controlId="characterProficiencyBonus" className="d-flex align-items-center justify-content-center w-100 h-100">
            <Form.Control readOnly value={proficiencyBonus} className="border-0 bg-transparent shadow-none text-center" />
          </Form.Group>
        </Card.ImgOverlay>
      </Card>
      <Card className="d-flex flex-column justify-content-center border-0">
        <Card.Img src={ProficiencyBonusSVG2} />
        <Card.ImgOverlay className="d-flex flex-column justify-content-center p-0 m-0">
          <Form.Group controlId="characterProficiencyBonus" className="text-center text-truncate">
            <Form.Label className="text-uppercase fw-bold p-0 m-0 label-bonus">Бонус мастерства</Form.Label>
          </Form.Group>
        </Card.ImgOverlay>
      </Card>
    </Card>
  );
}

export default ProficiencyBonusCard;