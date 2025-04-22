import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';
import Table from 'react-bootstrap/Table';
import Collapse from 'react-bootstrap/Collapse';

import { FaCog, FaTrash, FaLock, FaLockOpen, FaPlus } from 'react-icons/fa';

import { useState, Fragment, useMemo, useEffect } from 'react';
import { ToolProficienciesAndCustomSkillsCardEditableRowPropsType, AbilityType, ProficiencyType, RootState, Tool } from '../../types/state';
import { useAppDispatch, useAppSelector } from '../../hooks';
import { addTool, deleteTool, updateTool } from '../../store/sheet1Slice';

import { v4 as uuidv4 } from 'uuid';

const EditableRow: React.FC<ToolProficienciesAndCustomSkillsCardEditableRowPropsType> = ({ tool, isEditMode, onDelete }) => {
  const dispatch = useAppDispatch();
  const level = useAppSelector((state: RootState) => state.sheet1.level);
  const abilityBase = useAppSelector((state: RootState) => state.sheet1.abilities[tool.bondAbility]?.base ?? 10);

  const [isExpanded, setExpanded] = useState(false);

  // Сбрасываем isExpanded при смене isEditMode на false (режим удаления)
  useEffect(() => {
    if (!isEditMode) {
      setExpanded(false);
    }
  }, [isEditMode]);

  const [name, setName] = useState(tool.name);
  const [proficiencyType, setProficiencyType] = useState<ProficiencyType>(tool.proficiencyType);
  const [bondAbility, setBondAbility] = useState<AbilityType>(tool.bondAbility);
  const [mods, setMods] = useState(tool.mods);

  const toggleExpand = () => setExpanded((prev) => !prev);

  // Расчёт бонуса мастерства (Proficiency Bonus, PB)
  const calculatePB = (chLevel: number) => {
    if (chLevel >= 1 && chLevel <= 4) return 2;
    if (chLevel >= 5 && chLevel <= 8) return 3;
    if (chLevel >= 9 && chLevel <= 12) return 4;
    if (chLevel >= 13 && chLevel <= 16) return 5;
    if (chLevel >= 17 && chLevel <= 20) return 6;
    return 0;
  };

  // Модификатор характеристики
  const calculateAM = (abilityBase: number) => {
    return Math.floor((abilityBase - 10) / 2);
  };

  // Множитель для типа владения
  const calcPbMultiplier = (pType: ProficiencyType) => {
    if (pType === 'proficient') return 1;
    if (pType === 'expertise') return 2;
    if (pType === 'jackOfAllTrades') return 0.5;
    return 0;
  };

  // Расчёт итогового бонуса владения
  const calculateProficiency = (level: number, proficiencyType: ProficiencyType, abilityBase: number, mods: number) => {
    const pb = calculatePB(level);
    const multiplier = calcPbMultiplier(proficiencyType);
    const am = calculateAM(abilityBase);
    return am + Math.floor(pb * multiplier) + mods;
  };

  const calculatedProficiency = useMemo(
    () => calculateProficiency(level, proficiencyType, abilityBase, mods),
    [level, proficiencyType, abilityBase, mods]
  );

  const handleNameChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newName = event.target.value;
    setName(newName);
    dispatch(updateTool({ ...tool, name: newName }));
  };

  const handleProficiencyTypeChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const newProficiencyType = event.target.value as ProficiencyType;
    setProficiencyType(newProficiencyType);
    dispatch(updateTool({ ...tool, proficiencyType: newProficiencyType }));
  };

  const handleAbilityChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const newBondAbility = event.target.value as AbilityType;
    setBondAbility(newBondAbility);
    dispatch(updateTool({ ...tool, bondAbility: newBondAbility }));
  };

  const handleModsChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newMods = parseInt(event.target.value) || 0;
    setMods(newMods);
    dispatch(updateTool({ ...tool, mods: newMods }));
  };

  const displayAttribute = (bondAbility: AbilityType) => {
    switch (bondAbility.toLowerCase()) {
      case 'strength':
        return 'Сила';
      case 'dexterity':
        return 'Ловкость';
      case 'constitution':
        return 'Телосложение';
      case 'intelligence':
        return 'Интеллект';
      case 'wisdom':
        return 'Мудрость';
      case 'charisma':
        return 'Харизма';
      case 'queryattribute':
        return 'Другая';
      default:
        return 'Ошибка';
    }
  };

  return (
    <Fragment>
      <Row className="align-items-center py-1">
        <Col md={4} className="">
          {tool.name}
        </Col>
        <Col md={4} className="text-center">
          {calculatedProficiency >= 0 ? `+${calculatedProficiency}` : calculatedProficiency}
        </Col>
        <Col md={3}>{displayAttribute(tool.bondAbility)}</Col>
        <Col md={1} className="text-center">
          {isEditMode ? (
            <FaCog
              onClick={toggleExpand}
              style={{ cursor: 'pointer' }}
              aria-label="Редактировать умение"
            />
          ) : (
            <FaTrash
              onClick={onDelete}
              style={{ cursor: 'pointer', color: 'red' }}
              aria-label="Удалить умение"
            />
          )}
        </Col>
      </Row>
      <Collapse in={isExpanded}>
        <div className="p-3">
          <Form.Group controlId={`tool-${tool.id}-nameEdit`} className="mb-2">
            <Form.Label>Имя</Form.Label>
            <Form.Control
              type="text"
              value={name}
              onChange={handleNameChange}
              placeholder="Например, Кузнечные инструменты"
            />
          </Form.Group>
          <Form.Group controlId={`tool-${tool.id}-proficiencyTypeEdit`} className="mb-2">
            <Form.Label>Уровень владения</Form.Label>
            <Form.Select value={proficiencyType} onChange={handleProficiencyTypeChange}>
              <option value="none">Нет</option>
              <option value="proficient">Владение</option>
              <option value="expertise">Экспертиза</option>
              <option value="jackOfAllTrades">Мастер на все руки</option>
            </Form.Select>
          </Form.Group>
          <Form.Group controlId={`tool-${tool.id}-bondAttributeEdit`} className="mb-2">
            <Form.Label>Характеристика</Form.Label>
            <Form.Select value={bondAbility} onChange={handleAbilityChange}>
              <option value="strength">Сила</option>
              <option value="dexterity">Ловкость</option>
              <option value="constitution">Телосложение</option>
              <option value="intelligence">Интеллект</option>
              <option value="wisdom">Мудрость</option>
              <option value="charisma">Харизма</option>
              <option value="queryAttribute">Другая</option>
            </Form.Select>
          </Form.Group>
          <Form.Group controlId={`tool-${tool.id}-modsEdit`} className="mb-2">
            <Form.Label>Бонус</Form.Label>
            <Form.Control
              type="number"
              value={mods}
              onChange={handleModsChange}
              placeholder="0"
            />
          </Form.Group>
        </div>
      </Collapse>
    </Fragment>
  );
};

const ToolProficienciesAndCustomSkillsCard: React.FC = () => {
  const dispatch = useAppDispatch();
  const tools = useAppSelector((state: RootState) => state.sheet1.tools);

  const [isEditMode, setEditMode] = useState(true); // true -> edit mode, false -> deletion mode

  const toggleEditMode = () => setEditMode((prev) => !prev);

  const addNewTool = () => {
    const newTool: Tool = {
      id: uuidv4(),
      name: 'Новое умение',
      proficiencyType: 'proficient',
      bondAbility: 'strength',
      mods: 0,
    };
    dispatch(addTool(newTool));
  };

  const deleteToolById = (id: string) => () => dispatch(deleteTool(id));

  return (
    <Card className="p-0 m-0 border-0 w-100">
      <Card.Body className="p-3">
        <Container fluid className="p-3 border rounded">
          <Row className="fw-bold mb-2">
            <Col md={4}>Имя</Col>
            <Col md={4} className="text-center">Владение</Col>
            <Col md={3}>Хар-ка</Col>
            <Col md={1}></Col>
          </Row>
          {tools.length > 0 ? (
            tools.map((tool) => (
              <EditableRow
                key={tool.id}
                tool={tool}
                isEditMode={isEditMode}
                onDelete={deleteToolById(tool.id)}
              />
            ))
          ) : (
            <Row className="py-2">
              <Col className="text-center text-muted">Нет умений или владений</Col>
            </Row>
          )}
          <Row className="mt-3">
            <Col className="d-flex justify-content-center gap-3">
              {isEditMode ? (
                <>
                  <FaPlus
                    onClick={addNewTool}
                    style={{ cursor: 'pointer'}}
                    aria-label="Добавить новое умение"
                  />
                  <FaLock
                    onClick={toggleEditMode}
                    style={{ cursor: 'pointer'}}
                    aria-label="Переключить в режим удаления"
                  />
                </>
              ) : (
                <FaLockOpen
                  onClick={toggleEditMode}
                  style={{ cursor: 'pointer'}}
                  aria-label="Переключить в режим редактирования"
                />
              )}
            </Col>
          </Row>
        </Container>
        <div className="p-0 m-0 mt-2 text-center">
          <span className="text-uppercase fw-bold">Владение снаряжением и Умения</span>
        </div>
      </Card.Body>
    </Card>
  );
}

export default ToolProficienciesAndCustomSkillsCard;