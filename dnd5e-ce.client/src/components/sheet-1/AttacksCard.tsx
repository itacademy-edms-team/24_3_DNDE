import Col from 'react-bootstrap/Col';
import Collapse from 'react-bootstrap/Collapse';
import Container from 'react-bootstrap/Container';
import Form from 'react-bootstrap/Form';
import Row from 'react-bootstrap/Row';

import { FaCog, FaLock, FaLockOpen, FaPlus, FaTrash } from 'react-icons/fa';

import { v4 as uuidv4 } from 'uuid';

import { Fragment, useEffect, useState } from 'react';
import { useAppDispatch, useAppSelector } from '../../hooks';
import { selectAttacks } from '../../store/selectors/sheet1Selectors';
import { addAttack, deleteAttack, updateAttack } from '../../store/sheet1Slice';
import { AbilityType, Attack, AttacksCardAttackEditableRowPropsType, DCAbilityType, DamageAbilityType, RootState } from '../../types/state';

const AttackEditableRow: React.FC<AttacksCardAttackEditableRowPropsType> = ({
  attack,
  isEditMode,
  onDelete
}) => {
  const dispatch = useAppDispatch();
  const abilities = useAppSelector((state: RootState) => state.sheet1.abilities);
  const level = useAppSelector((state: RootState) => state.sheet1.level);

  const [isExpanded, setExpanded] = useState(false);

  // Сворачиваем форму при смене режима
  useEffect(() => {
    if (!isEditMode) {
      setExpanded(false);
    }
  }, [isEditMode]);
  
  const toggleExpand = () => setExpanded((prev) => !prev);

  const updateAttackField = (updates: Partial<Attack>) => {
    dispatch(updateAttack({ id: attack.id, ...updates }));
  };

  // Обработчики изменений
  const handleAttackNameChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateAttackField({ name: event.target.value });
  };

  const handleIsAtkIncludedChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateAttackField({ atk: { ...attack.atk, isIncluded: event.target.checked } });
  };

  const handleAtkBondAbilityChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    updateAttackField({ atk: { ...attack.atk, bondAbility: event.target.value as AbilityType } });
  };

  const handleAtkBonusChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(event.target.value); // TODO: Add checks
    if (!isNaN(newValue)) {
      updateAttackField({ atk: { ...attack.atk, bonus: newValue } });
    }
  };

  const handleIsAtkProficientChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateAttackField({ atk: { ...attack.atk, isProficient: event.target.checked } });
  };

  const handleAtkRangeChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateAttackField({ atk: { ...attack.atk, range: event.target.value } });
  };

  const handleAtkMagicBonusChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(event.target.value); // TODO: Add checks
    if (!isNaN(newValue)) {
      updateAttackField({ atk: { ...attack.atk, magicBonus: newValue } });
    }
  };

  const handleAtkCritRangeChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateAttackField({ atk: { ...attack.atk, critRange: event.target.value } });
  };

  const handleDamage1IsIncludedChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateAttackField({ damage1: { ...attack.damage1, isIncluded: event.target.checked } });
  };

  const handleDamage1DiceChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateAttackField({ damage1: { ...attack.damage1, dice: event.target.value } });
  };

  const handleDamage1BondAbilityChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    updateAttackField({ damage1: { ...attack.damage1, bondAbility: event.target.value as AbilityType } });
  };

  const handleDamage1BonusChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(event.target.value); // TODO: Add checks
    if (!isNaN(newValue)) {
      updateAttackField({ damage1: { ...attack.damage1, bonus: newValue } });
    }
  };

  const handleDamage1TypeChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateAttackField({ damage1: { ...attack.damage1, type: event.target.value } });
  };

  const handleDamage1CritDiceChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateAttackField({ damage1: { ...attack.damage1, critDice: event.target.value } });
  };

  const handleDamage2IsIncludedChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateAttackField({ damage2: { ...attack.damage2, isIncluded: event.target.checked } });
  };

  const handleDamage2DiceChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateAttackField({ damage2: { ...attack.damage2, dice: event.target.value } });
  };

  const handleDamage2BondAbilityChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    updateAttackField({ damage2: { ...attack.damage2, bondAbility: event.target.value as DamageAbilityType } });
  };

  const handleDamage2BonusChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(event.target.value); // TODO: Add checks
    if (!isNaN(newValue)) {
      updateAttackField({ damage2: { ...attack.damage2, bonus: newValue } });
    }
  };

  const handleDamage2TypeChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateAttackField({ damage2: { ...attack.damage2, type: event.target.value } });
  };

  const handleDamage2CritDiceChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateAttackField({ damage2: { ...attack.damage2, critDice: event.target.value } });
  };

  const handleSavingThrowIsIncludedChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateAttackField({ savingThrow: { ...attack.savingThrow, isIncluded: event.target.checked } });
  };

  const handleSavingThrowBondAbilityChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    updateAttackField({ savingThrow: { ...attack.savingThrow, bondAbility: event.target.value as AbilityType } });
  };

  const handleSavingThrowDifficultyClassChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    updateAttackField({ savingThrow: { ...attack.savingThrow, dificultyClass: event.target.value as DCAbilityType } });
  };

  const handleSaveEffectChange = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
    updateAttackField({ saveEffect: event.target.value });
  };

  const handleDescriptionChange = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
    updateAttackField({ description: event.target.value });
  };

  // Расчёт бонуса атаки
  const calculateAttackBonus = () => {
    let bonus = 0;
    if (attack.atk.isIncluded && attack.atk.bondAbility !== '-') {
      const abilityMod = attack.atk.bondAbility !== "spell" ?
        Math.floor((abilities[attack.atk.bondAbility]?.base - 10) / 2)
        : 0; // TODO: Fix later when add Spells
      bonus += abilityMod;
    }
    if (attack.atk.isProficient) {
      const proficiencyBonus = Math.floor((level + 7) / 4); // 2 at level 1, 3 at level 5, etc.
      bonus += proficiencyBonus;
    }
    bonus += attack.atk.bonus + attack.atk.magicBonus;
    return bonus;
  };

  // Расчёт урона (damage1 и damage2)
  const calculateDamage = (damage: Attack['damage1']) => {
    if (!damage.isIncluded) return "";
    let result = damage.dice;
    if (damage.bondAbility === "spell") return NaN; // TODO: Fix when add Spells
    if (damage.bondAbility !== '-' && abilities[damage.bondAbility]) {
      const mod = Math.floor((abilities[damage.bondAbility].base - 10) / 2);
      result += `${mod >= 0 ? '+' : ''}${mod}`;
    }
    if (damage.bonus !== 0) {
      result += `${damage.bonus >= 0 ? '+' : ''}${damage.bonus}`;
    }
    return result;
  };

  // Отображение урона
  const displayDamage = () => {
    const damages: string[] = [];
    if (attack.damage1.isIncluded && attack.damage1.dice) {
      damages.push(`${calculateDamage(attack.damage1)} ${attack.damage1.type ? `(${attack.damage1.type})` : ''}`);
    }
    if (attack.damage2.isIncluded && attack.damage2.dice) {
      damages.push(`${calculateDamage(attack.damage2)} ${attack.damage2.type ? `(${attack.damage2.type})` : ''}`);
    }
    return damages.join(' + ') || '';
  };

  // Расчёт сложности спасброска
  const calculateDifficultyClass = () => {
    if (!attack.savingThrow.isIncluded) return '-';
    if (attack.savingThrow.dificultyClass === 'flat') return '10';
    if (attack.savingThrow.dificultyClass === 'spell') {
      //const mod = Math.floor((abilities.spell.base - 10) / 2);
      //const proficiencyBonus = Math.floor((level + 7) / 4);
      //return `${8 + mod + proficiencyBonus}`;
      return NaN; // TODO: Fix later when add Spells
    }
    if (abilities[attack.savingThrow.dificultyClass]) {
      const mod = Math.floor((abilities[attack.savingThrow.dificultyClass].base - 10) / 2);
      const proficiencyBonus = Math.floor((level + 7) / 4);
      return `${8 + mod + proficiencyBonus}`;
    }
    return '-';
  };

  return (
    <Fragment>
      <Row className="pt-3 align-items-center">
        <Col md={4} className="text-truncate">
          {attack.name}
        </Col>
        <Col md={3} className="text-center">
          {attack.atk.isIncluded ? `${calculateAttackBonus() >= 0 ? '+' : ''}${calculateAttackBonus()}` : '-'}
        </Col>
        <Col md={4} className="text-truncate">
          {displayDamage()}
        </Col>
        <Col md={1} className="text-center">
          {isEditMode ? (
            <FaCog
              onClick={toggleExpand}
              style={{ cursor: 'pointer' }}
              aria-label={`Редактировать атаку ${attack.name}`}
            />
          ) : (
            <FaTrash
              onClick={onDelete}
              style={{ cursor: 'pointer' }}
              aria-label={`Удалить атаку ${attack.name}`}
            />
          )}
        </Col>
      </Row>
      <Collapse in={isExpanded}>
        <div className="p-3">
          <Form.Group controlId={`attack-${attack.id}-editName`} className="mb-2">
            <Form.Label className="attack-form-label">Имя</Form.Label>
            <Form.Control
              type="text"
              value={attack.name}
              onChange={handleAttackNameChange}
              placeholder="Например, Длинный меч"
              aria-label="Название атаки"
            />
          </Form.Group>
          <Row className="mb-2">
            <Col md={1}>
              <Form.Check
                id={`attack-${attack.id}-isAtkIncluded`}
                checked={attack.atk.isIncluded}
                onChange={handleIsAtkIncludedChange}
                aria-label="Включить бонус атаки"
              />
            </Col>
            <Col md={11}>
              <Form.Group controlId={`attack-${attack.id}-editAtkBondAbility`} className="d-flex align-items-center gap-2 mb-2">
                <Form.Label className="attack-form-label m-0">Связанная способность:</Form.Label>
                <Form.Select
                  value={attack.atk.bondAbility}
                  onChange={handleAtkBondAbilityChange}
                  aria-label="Выберите связанную способность для атаки"
                >
                  <option value="strength">СИЛ</option>
                  <option value="dexterity">ЛОВ</option>
                  <option value="constitution">ТЕЛ</option>
                  <option value="intelligence">ИНТ</option>
                  <option value="wisdom">МУД</option>
                  <option value="charisma">ХАР</option>
                  <option value="spell">Заклинание</option>
                  <option value="-">-</option>
                </Form.Select>
              </Form.Group>
              <div className="d-flex align-items-center gap-2 mb-2">
                <Form.Group controlId={`attack-${attack.id}-editAtkBonus`} style={{ width: '4rem' }}>
                  <Form.Label className="attack-form-label">Бонус:</Form.Label>
                  <Form.Control
                    type="number"
                    value={attack.atk.bonus}
                    onChange={handleAtkBonusChange}
                    aria-label="Дополнительный бонус атаки"
                  />
                </Form.Group>
                <Form.Check
                  id={`attack-${attack.id}-isAtkProficient`}
                  label="Владение"
                  checked={attack.atk.isProficient}
                  onChange={handleIsAtkProficientChange}
                  aria-label="Владение атакой"
                />
              </div>
              <Form.Group controlId={`attack-${attack.id}-editRange`} className="mb-2">
                <Form.Label className="attack-form-label">Дальность:</Form.Label>
                <Form.Control
                  type="text"
                  value={attack.atk.range}
                  onChange={handleAtkRangeChange}
                  placeholder="Например, 5 футов или 30/120 футов"
                  aria-label="Дальность атаки"
                />
              </Form.Group>
              <Form.Group controlId={`attack-${attack.id}-editAtkMagicBonus`} className="mb-2">
                <Form.Label className="attack-form-label">Магический бонус:</Form.Label>
                <Form.Control
                  type="number"
                  value={attack.atk.magicBonus}
                  onChange={handleAtkMagicBonusChange}
                  aria-label="Магический бонус атаки"
                />
              </Form.Group>
              <Form.Group controlId={`attack-${attack.id}-editAtkCritRange`} className="mb-2">
                <Form.Label className="attack-form-label">Критический диапазон:</Form.Label>
                <Form.Control
                  type="text"
                  value={attack.atk.critRange}
                  onChange={handleAtkCritRangeChange}
                  placeholder="Например, 19-20"
                  aria-label="Критический диапазон атаки"
                />
              </Form.Group>
            </Col>
          </Row>
          <h6 className="attack-form-section-title">Урон 1</h6>
          <Row className="mb-2">
            <Col md={1}>
              <Form.Check
                id={`attack-${attack.id}-damage1IsIncluded`}
                checked={attack.damage1.isIncluded}
                onChange={handleDamage1IsIncludedChange}
                aria-label="Включить первый урон"
              />
            </Col>
            <Col md={11}>
              <Form.Group controlId={`attack-${attack.id}-editDamage1Dice`} className="mb-2">
                <Form.Label className="attack-form-label">Кубик урона:</Form.Label>
                <Form.Control
                  type="text"
                  value={attack.damage1.dice}
                  onChange={handleDamage1DiceChange}
                  placeholder="Например, 1d8"
                  aria-label="Кубики первого урона"
                />
              </Form.Group>
              <Form.Group controlId={`attack-${attack.id}-editDamage1BondAbility`} className="d-flex align-items-center gap-2 mb-2">
                <Form.Label className="attack-form-label m-0">Связанная способность:</Form.Label>
                <Form.Select
                  value={attack.damage1.bondAbility}
                  onChange={handleDamage1BondAbilityChange}
                  aria-label="Выберите связанную способность для первого урона"
                >
                  <option value="strength">СИЛ</option>
                  <option value="dexterity">ЛОВ</option>
                  <option value="constitution">ТЕЛ</option>
                  <option value="intelligence">ИНТ</option>
                  <option value="wisdom">МУД</option>
                  <option value="charisma">ХАР</option>
                  <option value="spell">Заклинание</option>
                  <option value="-">-</option>
                </Form.Select>
              </Form.Group>
              <Form.Group controlId={`attack-${attack.id}-editDamage1Bonus`} className="mb-2" style={{ width: '4rem' }}>
                <Form.Label className="attack-form-label">Бонус:</Form.Label>
                <Form.Control
                  type="number"
                  value={attack.damage1.bonus}
                  onChange={handleDamage1BonusChange}
                  aria-label="Бонус первого урона"
                />
              </Form.Group>
              <Form.Group controlId={`attack-${attack.id}-editDamage1Type`} className="mb-2">
                <Form.Label className="attack-form-label">Тип урона:</Form.Label>
                <Form.Control
                  type="text"
                  value={attack.damage1.type}
                  onChange={handleDamage1TypeChange}
                  placeholder="Например, рубящий"
                  aria-label="Тип первого урона"
                />
              </Form.Group>
              <Form.Group controlId={`attack-${attack.id}-editDamage1CritDice`} className="mb-2">
                <Form.Label className="attack-form-label">Критический кубик:</Form.Label>
                <Form.Control
                  type="text"
                  value={attack.damage1.critDice}
                  onChange={handleDamage1CritDiceChange}
                  placeholder="Например, 1d6"
                  aria-label="Критические кубики первого урона"
                />
              </Form.Group>
            </Col>
          </Row>
          <h6 className="attack-form-section-title">Урон 2</h6>
          <Row className="mb-2">
            <Col md={1}>
              <Form.Check
                id={`attack-${attack.id}-damage2IsIncluded`}
                checked={attack.damage2.isIncluded}
                onChange={handleDamage2IsIncludedChange}
                aria-label="Включить второй урон"
              />
            </Col>
            <Col md={11}>
              <Form.Group controlId={`attack-${attack.id}-editDamage2Dice`} className="mb-2">
                <Form.Label className="attack-form-label">Кубик урона:</Form.Label>
                <Form.Control
                  type="text"
                  value={attack.damage2.dice}
                  onChange={handleDamage2DiceChange}
                  placeholder="Например, 1d6"
                  aria-label="Кубики второго урона"
                />
              </Form.Group>
              <Form.Group controlId={`attack-${attack.id}-editDamage2BondAbility`} className="d-flex align-items-center gap-2 mb-2">
                <Form.Label className="attack-form-label m-0">Связанная способность:</Form.Label>
                <Form.Select
                  value={attack.damage2.bondAbility}
                  onChange={handleDamage2BondAbilityChange}
                  aria-label="Выберите связанную способность для второго урона"
                >
                  <option value="strength">СИЛ</option>
                  <option value="dexterity">ЛОВ</option>
                  <option value="constitution">ТЕЛ</option>
                  <option value="intelligence">ИНТ</option>
                  <option value="wisdom">МУД</option>
                  <option value="charisma">ХАР</option>
                  <option value="spell">Заклинание</option>
                  <option value="-">-</option>
                </Form.Select>
              </Form.Group>
              <Form.Group controlId={`attack-${attack.id}-editDamage2Bonus`} className="mb-2" style={{ width: '4rem' }}>
                <Form.Label className="attack-form-label">Бонус:</Form.Label>
                <Form.Control
                  type="number"
                  value={attack.damage2.bonus}
                  onChange={handleDamage2BonusChange}
                  aria-label="Бонус второго урона"
                />
              </Form.Group>
              <Form.Group controlId={`attack-${attack.id}-editDamage2Type`} className="mb-2">
                <Form.Label className="attack-form-label">Тип урона:</Form.Label>
                <Form.Control
                  type="text"
                  value={attack.damage2.type}
                  onChange={handleDamage2TypeChange}
                  placeholder="Например, огненный"
                  aria-label="Тип второго урона"
                />
              </Form.Group>
              <Form.Group controlId={`attack-${attack.id}-editDamage2CritDice`} className="mb-2">
                <Form.Label className="attack-form-label">Критический кубик:</Form.Label>
                <Form.Control
                  type="text"
                  value={attack.damage2.critDice}
                  onChange={handleDamage2CritDiceChange}
                  placeholder="Например, 1d6"
                  aria-label="Критические кубики второго урона"
                />
              </Form.Group>
            </Col>
          </Row>
          <h6 className="attack-form-section-title">Спасбросок</h6>
          <Row className="mb-2">
            <Col md={1}>
              <Form.Check
                id={`attack-${attack.id}-savingThrowIsIncluded`}
                checked={attack.savingThrow.isIncluded}
                onChange={handleSavingThrowIsIncludedChange}
                aria-label="Включить спасбросок"
              />
            </Col>
            <Col md={11}>
              <Form.Group controlId={`attack-${attack.id}-editSavingThrowBondAbility`} className="d-flex align-items-center gap-2 mb-2">
                <Form.Label className="attack-form-label m-0">Связанная способность:</Form.Label>
                <Form.Select
                  value={attack.savingThrow.bondAbility}
                  onChange={handleSavingThrowBondAbilityChange}
                  aria-label="Выберите связанную способность для спасброска"
                >
                  <option value="strength">СИЛ</option>
                  <option value="dexterity">ЛОВ</option>
                  <option value="constitution">ТЕЛ</option>
                  <option value="intelligence">ИНТ</option>
                  <option value="wisdom">МУД</option>
                  <option value="charisma">ХАР</option>
                  <option value="spell">Заклинание</option>
                </Form.Select>
              </Form.Group>
              <Form.Group controlId={`attack-${attack.id}-editSavingThrowDifficultyClass`} className="d-flex align-items-center gap-2 mb-2">
                <Form.Label className="attack-form-label m-0">Класс сложности:</Form.Label>
                <Form.Select
                  value={attack.savingThrow.dificultyClass}
                  onChange={handleSavingThrowDifficultyClassChange}
                  aria-label="Выберите класс сложности спасброска"
                >
                  <option value="strength">СИЛ</option>
                  <option value="dexterity">ЛОВ</option>
                  <option value="constitution">ТЕЛ</option>
                  <option value="intelligence">ИНТ</option>
                  <option value="wisdom">МУД</option>
                  <option value="charisma">ХАР</option>
                  <option value="spell">Заклинание</option>
                  <option value="flat">Фиксированный (10)</option>
                </Form.Select>
              </Form.Group>
            </Col>
          </Row>
          <Form.Group controlId={`attack-${attack.id}-editSaveEffect`} className="mb-2">
            <Form.Label className="attack-form-label">Эффект спасброска:</Form.Label>
            <Form.Control
              as="textarea"
              rows={2}
              value={attack.saveEffect}
              onChange={handleSaveEffectChange}
              placeholder="Например, Половинный урон при успехе"
              aria-label="Эффект спасброска"
            />
          </Form.Group>
          <Form.Group controlId={`attack-${attack.id}-editDescription`} className="mb-2">
            <Form.Label className="attack-form-label">Описание:</Form.Label>
            <Form.Control
              as="textarea"
              rows={3}
              value={attack.description}
              onChange={handleDescriptionChange}
              placeholder="Например, Атака огненным шаром, требующая спасброска Ловкости"
              aria-label="Описание атаки"
            />
          </Form.Group>
        </div>
      </Collapse>
    </Fragment>
  );
}

const AttacksCard: React.FC = () => {
  const dispatch = useAppDispatch();
  const attacks = useAppSelector(selectAttacks);

  const [isEditMode, setEditMode] = useState(true); // false -> deletionMode
  const toggleEditMode = () => setEditMode(prev => !prev);

  const addNewAttack = () => {
    const newAttack: Attack = {
      id: uuidv4(),
      name: "Новая атака",
      atk: {
        isIncluded: true, // enable or disable participant in calculation
        bondAbility: "strength", // strength, dexterity, constitution, intelligence, wisdom, charisma, spell, "-"
        bonus: 0,
        isProficient: true,
        range: "",
        magicBonus: 0,
        critRange: ""
      },
      damage1: {
        isIncluded: true,
        dice: "1d6",
        bondAbility: "strength", // strength, dexterity, constitution, intelligence, wisdom, charisma, spell, "-"
        bonus: 0,
        type: "",
        critDice: ""
      },
      damage2: {
        isIncluded: false,
        dice: "",
        bondAbility: "-", // strength, dexterity, constitution, intelligence, wisdom, charisma, spell, "-"
        bonus: 0,
        type: "",
        critDice: ""
      },
      savingThrow: {
        isIncluded: false,
        bondAbility: "strength", // strength, dexterity, constitution, intelligence, wisdom, charisma
        dificultyClass: "strength" // strength, dexterity, constitution, intelligence, wisdom, charisma, spell, flat
      },
      saveEffect: "",
      description: ""
    }
    dispatch(addAttack(newAttack));
  }

  const deleteAttackById = (id: string) => {
    return () => dispatch(deleteAttack(id));
  };

  return (
    <>
      <Container className="border border-rounded">
        <Container fluid={true} className="p-1 m-0">
          <Row className="fw-bold">
            <Col md={4}>Имя</Col>
            <Col md={3}>Атака</Col>
            <Col md={4}>Урон/Тип</Col>
            <Col md={1}></Col>
          </Row>
          <Row>
            {attacks.length > 0 ? (
              attacks.map((attack) => (
                <AttackEditableRow
                  key={attack.id}
                  attack={attack}
                  isEditMode={isEditMode}
                  onDelete={deleteAttackById(attack.id)}
                />
              ))
            ) : (
              <Row className="py-2">
                <Col className="text-center text-muted">Нет атак</Col>
              </Row>
            )}
          </Row>
          <Row className="mt-3">
            <Col className="d-flex justify-content-center gap-3">
              {isEditMode ? (
                <>
                  <FaPlus
                    onClick={addNewAttack}
                    style={{ cursor: 'pointer' }}
                    aria-label="Добавить новую атаку"
                  />
                  <FaLock
                    onClick={toggleEditMode}
                    style={{ cursor: 'pointer' }}
                    aria-label="Переключить в режим удаления"
                  />
                </>
              ) : (
                <FaLockOpen
                  onClick={toggleEditMode}
                  style={{ cursor: 'pointer' }}
                  aria-label="Переключить в режим редактирования"
                />
              )}
            </Col>
          </Row>
        </Container>
        <div className="p-0 m-0 mt-2 text-center">
          <span className="text-uppercase fw-bold">Атаки и Заклинания</span>
        </div>
      </Container>
    </>
    
  );
}

export default AttacksCard;