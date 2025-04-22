import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import SavethrowsSVG from './assets/Savethrows.svg';

import { AbilityType, RootState, SaveThrowsPropsType } from '../../types/state';
import { useAppDispatch, useAppSelector } from '../../hooks';
import { useMemo } from 'react';

import { updateSaveThrowProficiency } from '../../store/sheet1Slice';

const SaveThrowCard: React.FC<SaveThrowsPropsType> = ({
  abilityName,
  cardData
}) => {
  const getCheckIdByAbility = (ability: AbilityType) => {
    const capitalizedAbility = ability.charAt(0).toUpperCase() + ability.slice(1);
    return `characterSaveThrow${capitalizedAbility}Check`;
  }

  const getResultIdByAbility = (ability: AbilityType) => {
    const capitalizedAbility = ability.charAt(0).toUpperCase() + ability.slice(1);
    return `characterSaveThrow${capitalizedAbility}Result`;
  }

  const calculateModifier = (abilityBase: number) => {
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
    const am = calculateModifier(abilityBase);
    const pb = isProficient ? calculatePB(chLevel) : 0;
    return am + pb + other
  }

  const dispatch = useAppDispatch();

  const abilityBase = useAppSelector((state: RootState) => state.sheet1.abilities[abilityName].base);
  const chLevel = useAppSelector((state: RootState) => state.sheet1.level);

  const isProficient = useAppSelector((state: RootState) => state.sheet1.saveThrows[abilityName].isProficient);
  const handleAbilityIsProficientChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.checked;
    dispatch(updateSaveThrowProficiency({ ability: abilityName, isProficient: newValue }));
  };

  const calculatedResult = useMemo(() => calculateResult(abilityBase, chLevel, isProficient), [abilityBase, chLevel, isProficient]);

  return (
    // text-truncate позволяет нормально ресайзить текст внутри блоков. Не удалять
    <Row className="d-flex flex-row border-0">
      <Col md={1} className="p-0 m-0">
        <Form.Group
          controlId={getCheckIdByAbility(abilityName)}
          className="p-0 m-0"
        >
          <Form.Check
            className="p-0 m-0"
            checked={isProficient}
            onChange={handleAbilityIsProficientChange}
          />
        </Form.Group>
      </Col>
      <Col md={3} className="p-0 m-0 flex-grow-1">
        <Form.Group controlId={getResultIdByAbility(abilityName)}>
          <Form.Control
            className="p-0 m-0 border-0 bg-transparent shadow-none text-center"
            value={calculatedResult}
            readOnly
          />
        </Form.Group>
      </Col>
      <Col md={8} className="p-0 m-0">
        <Form.Group controlId={getCheckIdByAbility(abilityName)} className="text-truncate">
          <Form.Label className="p-0 m-0">{cardData.label}</Form.Label>
        </Form.Group>
      </Col>
    </Row>
  );
}

const SaveThrowList: React.FC = () => {
  const saveThrowCardsData = [
    { abilityName: 'strength', label: 'Сила' },
    { abilityName: 'dexterity', label: 'Ловкость' },
    { abilityName: 'constitution', label: 'Телосложение' },
    { abilityName: 'intelligence', label: 'Интеллект' },
    { abilityName: 'wisdom', label: 'Мудрость' },
    { abilityName: 'charisma', label: 'Харизма' }
  ] as const;

  return (
    <Card className="p-0 m-0 border-0">
      <Card.Img src={SavethrowsSVG} />
      <Card.ImgOverlay className="p-0 px-2 d-flex flex-column gap-0 border-0">
        <Container className="px-3 flex-grow-1 gap-0">
          {saveThrowCardsData.map(({ abilityName, label }) => (
            <SaveThrowCard
              key={abilityName}
              abilityName={abilityName}
              cardData={{ label }}
            />
          ))}
        </Container>
        <span className="p-0 m-0 pb-1 border-0 text-center text-uppercase fw-bold">
          Спасброски
        </span>
      </Card.ImgOverlay>
    </Card>
  );
}

export default SaveThrowList;