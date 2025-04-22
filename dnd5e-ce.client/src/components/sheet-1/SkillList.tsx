import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import SkillsSVG from './assets/SkillsFrame.svg'

import { SkillType, RootState, SkillListPropsType } from '../../types/state';
import { useAppDispatch, useAppSelector } from '../../hooks';
import { useMemo } from 'react';

import { updateSkillProficiency } from '../../store/sheet1Slice';

const SkillCard: React.FC<SkillListPropsType> = ({
  skillName,
  cardData
}) => {

  const getCheckIdBySkill = (skill: SkillType) => {
    const capitalizedAbility = skill.charAt(0).toUpperCase() + skill.slice(1);
    return `characterSkill${capitalizedAbility}Check`;
  }

  const getResultIdBySkill = (skill: SkillType) => {
    const capitalizedAbility = skill.charAt(0).toUpperCase() + skill.slice(1);
    return `characterSkill${capitalizedAbility}Result`;
  }

  const calculateAM = (abilityBase: number) => {
    return Math.floor((abilityBase - 10) / 2);
  }

  const calculatePB = (level: number) => {
    if (level >= 1 && level <= 4) return 2;
    if (level >= 5 && level <= 8) return 3;
    if (level >= 9 && level <= 12) return 4;
    if (level >= 13 && level <= 16) return 5;
    if (level >= 17 && level <= 20) return 6;
    return 0;
  };

  const calculateResult = (abilityBase: number, chLevel: number, isProficient: boolean, other: number = 0) => {
    const am = calculateAM(abilityBase);
    const pb = isProficient ? calculatePB(chLevel) : 0;
    return am + pb + other
  }

  const dispatch = useAppDispatch();

  const abilityBase = useAppSelector((state: RootState) => state.sheet1.abilities[state.sheet1.skills[skillName].ability].base);
  const chLevel = useAppSelector((state: RootState) => state.sheet1.level);

  const isProficient = useAppSelector((state: RootState) => state.sheet1.skills[skillName].isProficient);
  const handleSkillIsProficientChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.checked;
    dispatch(updateSkillProficiency({ skill: skillName, isProficient: newValue }));
  };

  const calculatedResult = useMemo(() => calculateResult(abilityBase, chLevel, isProficient), [abilityBase, chLevel, isProficient]);
  
  return (
    <Col className="d-flex flex-row align-items-center border-0">
      <Col md={1} className="p-0 m-0">
        <Form.Group controlId={getCheckIdBySkill(skillName)} className="p-0 pt-0 pb-0 m-0">
          <Form.Check
            checked={isProficient}
            onChange={handleSkillIsProficientChange}
            className="p-0 m-0 label-bonus"
          />
        </Form.Group>
      </Col>
      <Col md={3} className="p-0 m-0">
        <Form.Group controlId={getResultIdBySkill(skillName)}>
          <Form.Control
            value={calculatedResult}
            readOnly
            className="p-0 m-0 border-0 bg-transparent shadow-none text-center" />
        </Form.Group>
      </Col>
      <Col md={8} className="p-0 m-0">
        <Form.Group controlId={getCheckIdBySkill(skillName)} className="text-truncate">
          <Form.Label className="p-0 m-0">{cardData.label}</Form.Label>
        </Form.Group>
      </Col>
    </Col>
  );
}

function SkillList() {
  const skillCardsData = [
  { skillName: "acrobatics", label: "Акробатика (Лов)" },
  { skillName: "athletics", label: "Атлетика (Сил)" },
  { skillName: "investigation", label: "Анализ (Инт)" },
  { skillName: "perception", label: "Внимательность (Муд)" },
  { skillName: "survival", label: "Выживание (Муд)" },
  { skillName: "performance", label: "Выступление (Хар)" },
  { skillName: "history", label: "История (Инт)" },
  { skillName: "intimidation", label: "Запугивание (Хар)" },
  { skillName: "sleightOfHand", label: "Ловкость рук (Лов)" },
  { skillName: "arcana", label: "Магия (Инт)" },
  { skillName: "medicine", label: "Медицина (Муд)" },
  { skillName: "nature", label: "Природа (Инт)" },
  { skillName: "insight", label: "Проницательность (Муд)" },
  { skillName: "religion", label: "Религия (Инт)" },
  { skillName: "stealth", label: "Скрытность (Лов)" },
  { skillName: "persuasion", label: "Убеждение (Хар)" },
  { skillName: "deception", label: "Обман (Хар)" },
  { skillName: "animalHandling", label: "Уход за животными (Муд)" }
] as const;

  return (
    <Card className="p-0 m-0 border-0">
      <Card.Img src={SkillsSVG} />
      <Card.ImgOverlay className="p-0 m-0 d-flex flex-column gap-0 border-0 ">
        <Container className="p-0 px-3 m-0 flex-grow-1">
          {skillCardsData.map(({ skillName, label }) => (
            <SkillCard
              key={skillName}
              skillName={skillName}
              cardData={{ label }}
            />
          ))}
        </Container>
        <span className="pb-1 border-0 text-center text-uppercase fw-bold">
          Навыки
        </span>
      </Card.ImgOverlay>
    </Card>
  );
}

export default SkillList;