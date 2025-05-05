import Card from 'react-bootstrap/Card';
import Collapse from 'react-bootstrap/Collapse';
import Container from 'react-bootstrap/Container';
import Form from 'react-bootstrap/Form';

import { FaCog, FaInfoCircle, FaLock, FaLockOpen, FaPlus, FaTrash } from 'react-icons/fa';

import { v4 as uuidv4 } from 'uuid';

import React, { ChangeEvent, useState } from 'react';
import { useAppDispatch, useAppSelector } from '../../hooks';
import { selectPactMagic, selectRemainingSpellSlots, selectSpellCastingAbility, selectSpellsByLevel, selectSpellSlots } from '../../store/selectors/sheet3Selectors';
import {
    addSpell,
    deleteSpell,
    updateCharacterSpellCastingAbility,
    updateRemainingSpellSlots,
    updateSpellAtHigherLevels,
    updateSpellAttackDamage1Dice,
    updateSpellAttackDamage1Type,
    updateSpellAttackDamage2Dice,
    updateSpellAttackDamage2Type,
    updateSpellAttackType,
    updateSpellCantripProgression,
    updateSpellCastingAbility,
    updateSpellCastingTime,
    updateSpellClass,
    updateSpellComponentsDescription,
    updateSpellComponentsM,
    updateSpellComponentsS,
    updateSpellComponentsV,
    updateSpellDescription,
    updateSpellDuration,
    updateSpellHealingDice,
    updateSpellHigherLevelCastBonus,
    updateSpellHigherLevelCastDiceAmount,
    updateSpellHigherLevelCastDiceType,
    updateSpellIncludeSpellDescriptionInAttack,
    updateSpellInnate,
    updateSpellIsAbilityModIncluded,
    updateSpellIsConcentration,
    updateSpellIsPrepared,
    updateSpellIsRitual,
    updateSpellName,
    updateSpellOutput,
    updateSpellRange,
    updateSpellSavingThrowAbility,
    updateSpellSavingThrowEffect,
    updateSpellSchool,
    updateSpellTarget,
    updateSpellType,
} from '../../store/sheet3Slice';
import { CantripProgressionType, Spell, SpellAbilityType, SpellAttackType, SpellCardPropsType, SpellDescriptionInAttackIncludeVariety, SpellHighLevelCastDiceType, SpellLevel, SpellOutputType, SpellSchool } from '../../types/state';
import './SpellItem.css';

const SpellItem: React.FC<SpellCardPropsType> = ({
  spell,
  isEditMode,
  onDelete,
  spellLevel,
}) => {
  const dispatch = useAppDispatch();
  const spellBondAbility = useAppSelector(selectSpellCastingAbility);

  const [isInfoExpanded, setInfoExpanded] = useState(false);
  const [isEditExpanded, setEditExpanded] = useState(false);

  const toggleIsInfoExpanded = () => setInfoExpanded(!isInfoExpanded);
  const toggleIsEditExpanded = () => setEditExpanded(!isEditExpanded);

  // Individual handlers for each field
  const handleNameChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellName({ id: spell.id, value: e.target.value, level: spellLevel }));
  };

  const handleSchoolChange = (e: ChangeEvent<HTMLSelectElement>) => {
    dispatch(updateSpellSchool({ id: spell.id, value: e.target.value as SpellSchool, level: spellLevel }));
  };

  const handleIsPreparedChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellIsPrepared({ id: spell.id, value: e.target.checked, level: spellLevel }));
  };

  const handleRitualChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellIsRitual({ id: spell.id, value: e.target.checked, level: spellLevel }));
  };

  const handleCastingTimeChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellCastingTime({ id: spell.id, value: e.target.value, level: spellLevel }));
  };

  const handleRangeChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellRange({ id: spell.id, value: e.target.value, level: spellLevel }));
  };

  const handleTargetChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellTarget({ id: spell.id, value: e.target.value, level: spellLevel }));
  };

  const handleComponentsVChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellComponentsV({ id: spell.id, value: e.target.checked, level: spellLevel }));
  };

  const handleComponentsSChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellComponentsS({ id: spell.id, value: e.target.checked, level: spellLevel }));
  };

  const handleComponentsMChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellComponentsM({ id: spell.id, value: e.target.checked, level: spellLevel }));
  };

  const handleComponentsDescriptionChange = (e: ChangeEvent<HTMLTextAreaElement>) => {
    dispatch(updateSpellComponentsDescription({ id: spell.id, value: e.target.value, level: spellLevel }));
  };

  const handleConcentrationChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellIsConcentration({ id: spell.id, value: e.target.checked, level: spellLevel }));
  };

  const handleDurationChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellDuration({ id: spell.id, value: e.target.value, level: spellLevel }));
  };

  const handleSpellCastingAbilityChange = (e: ChangeEvent<HTMLSelectElement>) => {
    dispatch(updateSpellCastingAbility({ id: spell.id, value: e.target.value as SpellAbilityType, level: spellLevel }));
  };

  const handleCharacterSpellCastingAbilityChange = (e: ChangeEvent<HTMLSelectElement>) => {
    dispatch(updateCharacterSpellCastingAbility(e.target.value as SpellAbilityType));
  };

  const handleInnateChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellInnate({ id: spell.id, value: e.target.value, level: spellLevel }));
  };

  const handleOutputChange = (e: ChangeEvent<HTMLSelectElement>) => {
    dispatch(updateSpellOutput({ id: spell.id, value: e.target.value as SpellOutputType, level: spellLevel }));
  };

  const handleAttackTypeChange = (e: ChangeEvent<HTMLSelectElement>) => {
    dispatch(updateSpellAttackType({ id: spell.id, value: e.target.value as SpellAttackType, level: spellLevel }));
  };

  const handleAttackDamage1DiceChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellAttackDamage1Dice({ id: spell.id, value: e.target.value, level: spellLevel }));
  };

  const handleAttackDamage1TypeChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellAttackDamage1Type({ id: spell.id, value: e.target.value, level: spellLevel }));
  };

  const handleAttackDamage2DiceChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellAttackDamage2Dice({ id: spell.id, value: e.target.value, level: spellLevel }));
  };

  const handleAttackDamage2TypeChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellAttackDamage2Type({ id: spell.id, value: e.target.value, level: spellLevel }));
  };

  const handleHealingDiceChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellHealingDice({ id: spell.id, value: e.target.value, level: spellLevel }));
  };

  const handleAbilityModIncludedChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellIsAbilityModIncluded({ id: spell.id, value: e.target.checked, level: spellLevel }));
  };

  const handleSavingThrowAbilityChange = (e: ChangeEvent<HTMLSelectElement>) => {
    dispatch(updateSpellSavingThrowAbility({ id: spell.id, value: e.target.value as SpellAbilityType, level: spellLevel }));
  };

  const handleSavingThrowEffectChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellSavingThrowEffect({ id: spell.id, value: e.target.value, level: spellLevel }));
  };

  const handleHigherLevelCastDiceAmountChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellHigherLevelCastDiceAmount({ id: spell.id, value: parseInt(e.target.value) || 0, level: spellLevel }));
  };

  const handleHigherLevelCastDiceTypeChange = (e: ChangeEvent<HTMLSelectElement>) => {
    dispatch(updateSpellHigherLevelCastDiceType({ id: spell.id, value: e.target.value as SpellHighLevelCastDiceType, level: spellLevel }));
  };

  const handleHigherLevelCastBonusChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellHigherLevelCastBonus({ id: spell.id, value: parseInt(e.target.value) || 0, level: spellLevel }));
  };

  const handleIncludeSpellDescriptionInAttackChange = (e: ChangeEvent<HTMLSelectElement>) => {
    dispatch(updateSpellIncludeSpellDescriptionInAttack({ id: spell.id, value: e.target.value as SpellDescriptionInAttackIncludeVariety, level: spellLevel }));
  };

  const handleDescriptionChange = (e: ChangeEvent<HTMLTextAreaElement>) => {
    dispatch(updateSpellDescription({ id: spell.id, value: e.target.value, level: spellLevel }));
  };

  const handleAtHigherLevelsChange = (e: ChangeEvent<HTMLTextAreaElement>) => {
    dispatch(updateSpellAtHigherLevels({ id: spell.id, value: e.target.value, level: spellLevel }));
  };

  const handleClassChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellClass({ id: spell.id, value: e.target.value, level: spellLevel }));
  };

  const handleTypeChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSpellType({ id: spell.id, value: e.target.value, level: spellLevel }));
  };

  const handleCantripProgressionChange = (e: ChangeEvent<HTMLSelectElement>) => {
    dispatch(updateSpellCantripProgression({ id: spell.id, value: e.target.value as CantripProgressionType, level: spellLevel }));
  };

  const getVSMString = (spell: Spell) => {
    const v = spell.components.v.isIncluded ? 'В' : '';
    const s = spell.components.s.isIncluded ? 'С' : '';
    const m = spell.components.m.isIncluded ? 'М' : '';
    return `${v}${s}${m}` || '';
  };

  const schoolTranslations: Record<SpellSchool, string> = {
    abjuration: 'Ограждение',
    conjuration: 'Вызов',
    divination: 'Прорицание',
    enchantment: 'Очарование',
    evocation: 'Воплощение',
    illusion: 'Иллюзия',
    necromancy: 'Некромантия',
    transmutation: 'Трансмутация',
  };

  return (
    <Container className="px-0">
      <Card>
        <Card.Header className="spell-header p-1 d-flex flex-row">
          <div className="flex-grow-1 position-relative">
            <Form.Control
              id={`spell-${spell.id}-name`}
              value={spell.name}
              readOnly
              aria-label={`Название ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
            />
            {isEditMode ? (
              <div
                className="position-absolute top-50 translate-middle-y d-flex gap-1"
                style={{ right: '0.5rem' }}
              >
                <FaCog
                  onClick={toggleIsEditExpanded}
                  className="spell-icon"
                  aria-label={`Редактировать ${spellLevel === 0 ? 'заговор' : 'заклинание'} ${spell.name}`}
                />
                <FaInfoCircle
                  onClick={toggleIsInfoExpanded}
                  className="spell-icon"
                  aria-label={`Информация о ${spellLevel === 0 ? 'заговоре' : 'заклинании'} ${spell.name}`}
                />
              </div>
            ) : (
              <FaTrash
                aria-label={`Удалить ${spellLevel === 0 ? 'заговор' : 'заклинание'} ${spell.name}`}
                onClick={onDelete}
                className="spell-icon position-absolute top-50 translate-middle-y"
                style={{ right: '0.5rem' }}
              />
            )}
          </div>
          <Form.Control
            id={`spell-${spell.id}-VSM`}
            value={getVSMString(spell)}
            readOnly
            style={{ width: '4rem' }}
          />
        </Card.Header>
      </Card>
      <Collapse in={isInfoExpanded}>
        <Card.Body className="p-1">
          <div className="d-flex flex-column gap-1">
            <div className="fst-italic" style={{ color: 'crimson' }}>
              {spell.name}
            </div>
            <div>
              <span className="fw-bold">Школа:</span>{' '}
              {schoolTranslations[spell.school]}
            </div>
            <div>
              <span className="fw-bold">Время каста:</span> {spell.castingTime}
            </div>
            <div>
              <span className="fw-bold">Дальность:</span> {spell.range}
            </div>
            <div>
              <span className="fw-bold">Цель:</span> {spell.target}
            </div>
            <div>
              <span className="fw-bold">Компоненты:</span> {getVSMString(spell)}
            </div>
            <div>
              <span className="fw-bold">Длительность:</span> {spell.duration}
            </div>
            <div>
              <span className="fw-bold">Описание:</span> {spell.description}
            </div>
          </div>
        </Card.Body>
      </Collapse>
      <Collapse in={isEditExpanded}>
        <Card.Body className="p-1">
          <div className="d-flex flex-column gap-1 text-nowrap">
            <Form.Group
              controlId={`spell-${spell.id}-name-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Название</Form.Label>
              <Form.Control
                size="sm"
                value={spell.name}
                onChange={handleNameChange}
                aria-label={`Название ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`spell-${spell.id}-school-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Школа</Form.Label>
              <Form.Select
                size="sm"
                value={spell.school}
                onChange={handleSchoolChange}
                aria-label={`Школа ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
              >
                <option value="abjuration">Ограждение</option>
                <option value="conjuration">Вызов</option>
                <option value="divination">Прорицание</option>
                <option value="enchantment">Очарование</option>
                <option value="evocation">Воплощение</option>
                <option value="illusion">Иллюзия</option>
                <option value="necromancy">Некромантия</option>
                <option value="transmutation">Трансмутация</option>
              </Form.Select>
            </Form.Group>
            {spellLevel > 0 && (
              <Form.Group
                controlId={`spell-${spell.id}-isPrepared-edit`}
                className="d-flex flex-row gap-1"
              >
                <Form.Label className="m-0 fw-bold">Подготовлено</Form.Label>
                <Form.Check
                  type="checkbox"
                  inline
                  checked={spell.isPrepared ?? false}
                  onChange={handleIsPreparedChange}
                  aria-label={`Подготовлено заклинание ${spell.name}`}
                />
              </Form.Group>
            )}
            <Form.Group
              controlId={`spell-${spell.id}-isRitual-edit`}
              className="d-flex flex-row gap-1"
            >
              <Form.Label className="m-0 fw-bold">Ритуальное</Form.Label>
              <Form.Check
                type="checkbox"
                inline
                checked={spell.isRitual}
                onChange={handleRitualChange}
                aria-label={`Ритуал ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`spell-${spell.id}-castingTime-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Время каста</Form.Label>
              <Form.Control
                size="sm"
                value={spell.castingTime}
                onChange={handleCastingTimeChange}
                aria-label={`Время каста ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`spell-${spell.id}-range-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Дальность</Form.Label>
              <Form.Control
                size="sm"
                value={spell.range}
                onChange={handleRangeChange}
                aria-label={`Дальность ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`spell-${spell.id}-target-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Цель</Form.Label>
              <Form.Control
                size="sm"
                value={spell.target}
                onChange={handleTargetChange}
                aria-label={`Цель ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
              />
            </Form.Group>
            <div className="d-flex flex-row gap-3 align-items-center">
              <div>
                <span className="fw-bold">Компоненты</span>
              </div>
              <Form.Group
                controlId={`spell-${spell.id}-components-v-edit`}
                className="d-flex flex-row gap-1 justify-content-center align-items-center"
              >
                <Form.Label className="m-0 fw-bold">В</Form.Label>
                <Form.Check
                  type="checkbox"
                  inline
                  checked={spell.components.v.isIncluded}
                  onChange={handleComponentsVChange}
                  aria-label={`Вербальный компонент ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
                  className="m-0"
                />
              </Form.Group>
              <Form.Group
                controlId={`spell-${spell.id}-components-s-edit`}
                className="d-flex flex-row gap-1 justify-content-center align-items-center"
              >
                <Form.Label className="m-0 fw-bold">С</Form.Label>
                <Form.Check
                  type="checkbox"
                  inline
                  checked={spell.components.s.isIncluded}
                  onChange={handleComponentsSChange}
                  aria-label={`Соматический компонент ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
                  className="m-0"
                />
              </Form.Group>
              <Form.Group
                controlId={`spell-${spell.id}-components-m-edit`}
                className="d-flex flex-row gap-1 justify-content-center align-items-center"
              >
                <Form.Label className="m-0 fw-bold">М</Form.Label>
                <Form.Check
                  type="checkbox"
                  inline
                  checked={spell.components.m.isIncluded}
                  onChange={handleComponentsMChange}
                  aria-label={`Материальный компонент ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
                  className="m-0"
                />
              </Form.Group>
            </div>
            <Form.Group
              controlId={`spell-${spell.id}-componentsDescription-edit`}
              className="d-flex flex-column gap-1 justify-content-center"
            >
              <Form.Label className="m-0 fw-bold">Описание компонентов</Form.Label>
              <Form.Control
                size="sm"
                as="textarea"
                rows={1}
                value={spell.componentsDescription}
                onChange={handleComponentsDescriptionChange}
                aria-label={`Описание компонентов ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`spell-${spell.id}-isConcentration-edit`}
              className="d-flex flex-row gap-1"
            >
              <Form.Label className="m-0 fw-bold">Требуется концентрация</Form.Label>
              <Form.Check
                type="checkbox"
                checked={spell.isConcentration}
                onChange={handleConcentrationChange}
                aria-label={`Концентрация ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`spell-${spell.id}-duration-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Длительность</Form.Label>
              <Form.Control
                size="sm"
                value={spell.duration}
                onChange={handleDurationChange}
                aria-label={`Длительность ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`spell-${spell.id}-spellCastingAbility-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Способность</Form.Label>
              <Form.Select
                size="sm"
                value={spell.spellCastingAbility}
                onChange={handleSpellCastingAbilityChange}
                aria-label={`Способность для каста ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
              >
                <option value="none">Нет</option>
                <option value="strength">Сила</option>
                <option value="dexterity">Ловкость</option>
                <option value="constitution">Телосложение</option>
                <option value="intelligence">Интеллект</option>
                <option value="wisdom">Мудрость</option>
                <option value="charisma">Харизма</option>
              </Form.Select>
            </Form.Group>
            <Form.Group
              controlId={`spell-${spell.id}-characterSpellCastingAbility-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Способность персонажа</Form.Label>
              <Form.Select
                size="sm"
                value={spellBondAbility}
                onChange={handleCharacterSpellCastingAbilityChange}
                aria-label={`Способность персонажа для каста`}
              >
                <option value="none">Нет</option>
                <option value="strength">Сила</option>
                <option value="dexterity">Ловкость</option>
                <option value="constitution">Телосложение</option>
                <option value="intelligence">Интеллект</option>
                <option value="wisdom">Мудрость</option>
                <option value="charisma">Харизма</option>
              </Form.Select>
            </Form.Group>
            <Form.Group
              controlId={`spell-${spell.id}-innate-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Кол-во использований</Form.Label>
              <Form.Control
                size="sm"
                value={spell.innate}
                onChange={handleInnateChange}
                placeholder="1/день"
                aria-label={`Врождённое ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`spell-${spell.id}-output-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Вывод</Form.Label>
              <Form.Select
                size="sm"
                value={spell.output}
                onChange={handleOutputChange}
                aria-label={`Вывод ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
              >
                <option value="spellcard">Карта заклинания</option>
                <option value="attack">Атака</option>
              </Form.Select>
            </Form.Group>
            {spell.output === "attack" ? (
              <>
                <Form.Group
                  controlId={`spell-${spell.id}-attack-attackType-edit`}
                  className="d-flex flex-row gap-1 justify-content-center align-items-center"
                >
                  <Form.Label className="m-0 fw-bold">Тип атаки</Form.Label>
                  <Form.Select
                    size="sm"
                    value={spell.attack.attackType}
                    onChange={handleAttackTypeChange}
                    aria-label={`Тип атаки ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
                  >
                    <option value="ranged">Дальний</option>
                    <option value="melee">Ближний</option>
                  </Form.Select>
                </Form.Group>
                <Form.Group
                  controlId={`spell-${spell.id}-attack-damage1-dice-edit`}
                  className="d-flex flex-row gap-1 justify-content-center align-items-center"
                >
                  <Form.Label className="m-0 fw-bold">Урон 1: Кости</Form.Label>
                  <Form.Control
                    size="sm"
                    value={spell.attack.damage1.dice}
                    onChange={handleAttackDamage1DiceChange}
                    aria-label={`Кости урона 1 ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
                  />
                </Form.Group>
                <Form.Group
                  controlId={`spell-${spell.id}-attack-damage1-type-edit`}
                  className="d-flex flex-row gap-1 justify-content-center align-items-center"
                >
                  <Form.Label className="m-0 fw-bold">Урон 1: Тип</Form.Label>
                  <Form.Control
                    size="sm"
                    value={spell.attack.damage1.type}
                    onChange={handleAttackDamage1TypeChange}
                    aria-label={`Тип урона 1 ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
                  />
                </Form.Group>
                <Form.Group
                  controlId={`spell-${spell.id}-attack-damage2-dice-edit`}
                  className="d-flex flex-row gap-1 justify-content-center align-items-center"
                >
                  <Form.Label className="m-0 fw-bold">Урон 2: Кости</Form.Label>
                  <Form.Control
                    size="sm"
                    value={spell.attack.damage2.dice}
                    onChange={handleAttackDamage2DiceChange}
                    aria-label={`Кости урона 2 ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
                  />
                </Form.Group>
                <Form.Group
                  controlId={`spell-${spell.id}-attack-damage2-type-edit`}
                  className="d-flex flex-row gap-1 justify-content-center align-items-center"
                >
                  <Form.Label className="m-0 fw-bold">Урон 2: Тип</Form.Label>
                  <Form.Control
                    size="sm"
                    value={spell.attack.damage2.type}
                    onChange={handleAttackDamage2TypeChange}
                    aria-label={`Тип урона 2 ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
                  />
                </Form.Group>
                <Form.Group
                  controlId={`spell-${spell.id}-healingDice-edit`}
                  className="d-flex flex-row gap-1 justify-content-center align-items-center"
                >
                  <Form.Label className="m-0 fw-bold">Лечащие кости</Form.Label>
                  <Form.Control
                    size="sm"
                    value={spell.healingDice}
                    onChange={handleHealingDiceChange}
                    aria-label={`Лечащие кости ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
                  />
                </Form.Group>
                <Form.Group
                  controlId={`spell-${spell.id}-isAbilityModIncluded-edit`}
                  className="d-flex flex-row gap-1 justify-content-center align-items-center"
                >
                  <Form.Label className="m-0 fw-bold text-wrap">
                    Учитывать модификатор способности
                  </Form.Label>
                  <Form.Check
                    type="checkbox"
                    checked={spell.isAbilityModIncluded}
                    onChange={handleAbilityModIncludedChange}
                    aria-label={`Модификатор способности ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
                  />
                </Form.Group>
                <Form.Group
                  controlId={`spell-${spell.id}-cantripProgression-edit`}
                  className="d-flex flex-row gap-1 justify-content-center align-items-center"
                >
                  <Form.Label className="m-0 fw-bold">Прогрессия</Form.Label>
                  <Form.Select
                    size="sm"
                    value={spell.cantripProgression}
                    onChange={handleCantripProgressionChange}
                    aria-label={`Прогрессия ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
                  >
                    <option value="none">Нет</option>
                    <option value="cantripBeam">Луч</option>
                    <option value="cantripDice">Кости</option>
                  </Form.Select>
                </Form.Group>
                <Form.Group
                  controlId={`spell-${spell.id}-savingThrow-ability-edit`}
                  className="d-flex flex-row gap-1 justify-content-center align-items-center"
                >
                  <Form.Label className="m-0 fw-bold">
                    Спасбросок: Способность
                  </Form.Label>
                  <Form.Select
                    size="sm"
                    value={spell.savingThrow.ability}
                    onChange={handleSavingThrowAbilityChange}
                    aria-label={`Способность спасброска ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
                  >
                    <option value="none">Нет</option>
                    <option value="strength">Сила</option>
                    <option value="dexterity">Ловкость</option>
                    <option value="constitution">Телосложение</option>
                    <option value="intelligence">Интеллект</option>
                    <option value="wisdom">Мудрость</option>
                    <option value="charisma">Харизма</option>
                  </Form.Select>
                </Form.Group>
                <Form.Group
                  controlId={`spell-${spell.id}-savingThrow-effect-edit`}
                  className="d-flex flex-row gap-1 justify-content-center align-items-center"
                >
                  <Form.Label className="m-0 fw-bold">Спасбросок: Эффект</Form.Label>
                  <Form.Control
                    size="sm"
                    value={spell.savingThrow.effect}
                    onChange={handleSavingThrowEffectChange}
                    aria-label={`Эффект спасброска ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
                  />
                </Form.Group>
                <Form.Group
                  controlId={`spell-${spell.id}-higherLevelCast-diceAmount-edit`}
                  className="d-flex flex-row gap-1 justify-content-center align-items-center"
                >
                  <Form.Label className="m-0 fw-bold text-wrap">
                    Каст на высоком уровне: Кол-во костей
                  </Form.Label>
                  <Form.Control
                    size="sm"
                    type="number"
                    value={spell.higherLevelCast.diceAmount}
                    onChange={handleHigherLevelCastDiceAmountChange}
                    aria-label={`Количество костей для каста на высоком уровне ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
                  />
                </Form.Group>
                <Form.Group
                  controlId={`spell-${spell.id}-higherLevelCast-diceType-edit`}
                  className="d-flex flex-row gap-1 justify-content-center align-items-center"
                >
                  <Form.Label className="m-0 fw-bold text-wrap">
                    Каст на высоком уровне: Тип костей
                  </Form.Label>
                  <Form.Select
                    size="sm"
                    value={spell.higherLevelCast.diceType}
                    onChange={handleHigherLevelCastDiceTypeChange}
                    aria-label={`Тип костей для каста на высоком уровне ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
                  >
                    <option value="none">Нет</option>
                    <option value="d4">d4</option>
                    <option value="d6">d6</option>
                    <option value="d8">d8</option>
                    <option value="d10">d10</option>
                    <option value="d12">d12</option>
                    <option value="d20">d20</option>
                  </Form.Select>
                </Form.Group>
                <Form.Group
                  controlId={`spell-${spell.id}-higherLevelCast-bonus-edit`}
                  className="d-flex flex-row gap-1 justify-content-center align-items-center"
                >
                  <Form.Label className="m-0 fw-bold">
                    Каст на высоком уровне: Бонус
                  </Form.Label>
                  <Form.Control
                    size="sm"
                    type="number"
                    value={spell.higherLevelCast.bonus}
                    onChange={handleHigherLevelCastBonusChange}
                    aria-label={`Бонус для каста на высоком уровне ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
                  />
                </Form.Group>
                <Form.Group
                  controlId={`spell-${spell.id}-includeSpellDescriptionInAttack-edit`}
                  className="d-flex flex-row gap-1 justify-content-center align-items-center"
                >
                  <Form.Label className="m-0 fw-bold">Описание в атаке</Form.Label>
                  <Form.Select
                    size="sm"
                    value={spell.includeSpellDescriptionInAttack}
                    onChange={handleIncludeSpellDescriptionInAttackChange}
                    aria-label={`Описание в атаке ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
                  >
                    <option value="off">Выкл</option>
                    <option value="partial">Частично</option>
                    <option value="on">Вкл</option>
                  </Form.Select>
                </Form.Group>
              </>
            ) : (
              null
            )}
            <Form.Group
              controlId={`spell-${spell.id}-description-edit`}
              className="d-flex flex-column gap-1"
            >
              <Form.Label className="m-0 fw-bold">Описание</Form.Label>
              <Form.Control
                size="sm"
                as="textarea"
                rows={3}
                value={spell.description}
                onChange={handleDescriptionChange}
                aria-label={`Описание ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`spell-${spell.id}-atHigherLevels-edit`}
              className="d-flex flex-column gap-1"
            >
              <Form.Label className="m-0 fw-bold">На высоких уровнях</Form.Label>
              <Form.Control
                size="sm"
                as="textarea"
                rows={3}
                value={spell.atHigherLevels}
                onChange={handleAtHigherLevelsChange}
                aria-label={`На высоких уровнях ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`spell-${spell.id}-class-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Класс</Form.Label>
              <Form.Control
                size="sm"
                value={spell.class}
                onChange={handleClassChange}
                aria-label={`Класс ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`spell-${spell.id}-type-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Тип</Form.Label>
              <Form.Control
                size="sm"
                value={spell.type}
                onChange={handleTypeChange}
                aria-label={`Тип ${spellLevel === 0 ? 'заговора' : 'заклинания'} ${spell.name}`}
              />
            </Form.Group>
          </div>
        </Card.Body>
      </Collapse>
    </Container>
  );
};

interface SpellCardComponentProps {
  spellLevel: SpellLevel;
  title: string;
  svgImage: string;
}

const SpellCardComponent: React.FC<SpellCardComponentProps> = ({
  spellLevel,
  title,
  svgImage,
}) => {
  const dispatch = useAppDispatch();
  const spells = useAppSelector(selectSpellsByLevel(spellLevel));
  const spellSlots = useAppSelector(selectSpellSlots);
  const pactMagic = useAppSelector(selectPactMagic);
  const remainingSpellSlots = useAppSelector(selectRemainingSpellSlots);

  const [isEditMode, setEditMode] = useState(true);
  const toggleEditMode = () => setEditMode((prev) => !prev);

  const addNewSpell = () => {
    const newSpell: Spell = {
      id: uuidv4(),
      name: spellLevel === 0 ? 'Новый заговор' : 'Новое заклинание',
      school: 'abjuration',
      isRitual: false,
      castingTime: '',
      range: '',
      target: '',
      components: {
        v: { isIncluded: true },
        s: { isIncluded: true },
        m: { isIncluded: true },
      },
      componentsDescription: '',
      isConcentration: false,
      duration: '',
      spellCastingAbility: 'none',
      innate: '',
      output: 'spellcard',
      attack: {
        attackType: 'ranged',
        damage1: { dice: '', type: '' },
        damage2: { dice: '', type: '' },
      },
      healingDice: '',
      isAbilityModIncluded: false,
      savingThrow: { ability: 'none', effect: '' },
      higherLevelCast: { diceType: 'none', diceAmount: 0, bonus: 0 },
      includeSpellDescriptionInAttack: 'off',
      description: '',
      atHigherLevels: '',
      class: '',
      type: '',
      cantripProgression: 'none',
      isPrepared: spellLevel > 0 ? false : true,
    };
    dispatch(addSpell({ spell: newSpell, level: spellLevel }));
  };

  const deleteSpellById = (spellId: string) => {
    dispatch(deleteSpell({ id: spellId, level: spellLevel }));
  };

  const handleRemainingSlotsChange = (e: ChangeEvent<HTMLInputElement>) => {
    const value = parseInt(e.target.value) || 0;
    dispatch(updateRemainingSpellSlots({ level: spellLevel, value }));
  };

  const totalSlots = pactMagic
    ? spellLevel === pactMagic.level
      ? pactMagic.slots
      : 0
    : spellSlots[spellLevel] || 0;

  return (
    <Container className="p-2 border rounded">
      {spellLevel === 1 ? (
        <div className="d-flex flex-row gap-3">
          <div style={{ width: "1rem" }} />
          <Form.Label className="fw-lighter">Всего слотов</Form.Label>
          <Form.Label className="fw-lighter">Оставшиеся слоты</Form.Label>
        </div>
      ) : (
        null
      )}
      <Card className="border-0">
        <Card.Img src={svgImage} />
        <Card.ImgOverlay className="px-1 h-100 d-flex justify-content-center align-items-center">
          {spellLevel === 0 ? (
            <div className="fw-bold text-wrap text-uppercase text-center">
              {title}
            </div>
          ) : (
            <div className="d-flex flex-row gap-3">
              <div style={{width: "1rem"}} />
              <Form.Group controlId={`spells-${spellLevel}-totalSlots`}>
                <Form.Control
                  size="sm"
                  value={totalSlots}
                  readOnly
                  aria-label={`Всего слотов для заклинаний ${spellLevel}-го уровня`}
                  className="text-center"
                  style={{width: "4rem"}}
                />
              </Form.Group>
              <Form.Group controlId={`spells-${spellLevel}-remainingSlots`}>
                <Form.Control
                  size="sm"
                  type="number"
                  value={remainingSpellSlots[`level${spellLevel}` as keyof typeof remainingSpellSlots]}
                  onChange={handleRemainingSlotsChange}
                  className="text-center"
                  aria-label={`Оставшиеся слоты для заклинаний ${spellLevel}-го уровня`}
                />
              </Form.Group>
            </div>
          )}
        </Card.ImgOverlay>
      </Card>
      <Container fluid className="px-0 pt-1">
        <Container className="px-0 d-flex flex-column gap-2">
          {spells.length === 0 ? (
            <div className="text-wrap fw-lighter">
              Нету {spellLevel === 0 ? 'заговоров' : 'заклинаний'}
            </div>
          ) : (
            spells.map((spell: Spell) => (
              <SpellItem
                key={spell.id}
                spell={spell}
                isEditMode={isEditMode}
                onDelete={() => deleteSpellById(spell.id)}
                spellLevel={spellLevel}
              />
            ))
          )}
        </Container>
      </Container>
      <Container className="mt-2 d-flex justify-content-between">
        {isEditMode ? (
          <>
            <FaPlus
              onClick={addNewSpell}
              style={{ cursor: 'pointer' }}
              aria-label={`Добавить новое ${spellLevel === 0 ? 'заговор' : 'заклинание'}`}
            />
            <FaLock
              onClick={toggleEditMode}
              style={{ cursor: 'pointer' }}
              aria-label="Переключить в режим удаления"
            />
          </>
        ) : (
          <>
            <div></div>
            <FaLockOpen
              onClick={toggleEditMode}
              style={{ cursor: 'pointer' }}
              aria-label="Переключить в режим редактирования"
            />
          </>
        )}
      </Container>
    </Container>
  );
};

export default SpellCardComponent;