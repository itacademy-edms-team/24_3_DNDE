import Card from 'react-bootstrap/Card';
import Col from 'react-bootstrap/Col';
import Container from 'react-bootstrap/Container';
import Form from 'react-bootstrap/Form';

import SkillsSVG from './assets/SkillsFrame.svg';

import { useAppDispatch, useAppSelector } from '../../hooks';
import { selectSkillValues } from '../../store/selectors/sheet1Selectors';
import { updateSkillProficiency } from '../../store/sheet1Slice';
import { RootState, SkillListPropsType, SkillType } from '../../types/state';

const SkillCard: React.FC<SkillListPropsType> = ({
  skillName,
  cardData
}) => {
  const dispatch = useAppDispatch();

  const result = useAppSelector(selectSkillValues)[skillName];

  const isProficient = useAppSelector((state: RootState) => state.sheet1.skills[skillName].isProficient);
  const handleSkillIsProficientChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.checked;
    dispatch(updateSkillProficiency({ skill: skillName, isProficient: newValue }));
  };

  const getCheckIdBySkill = (skill: SkillType) => {
    const capitalizedAbility = skill.charAt(0).toUpperCase() + skill.slice(1);
    return `characterSkill${capitalizedAbility}Check`;
  }

  const getResultIdBySkill = (skill: SkillType) => {
    const capitalizedAbility = skill.charAt(0).toUpperCase() + skill.slice(1);
    return `characterSkill${capitalizedAbility}Result`;
  }
  
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
            value={result}
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