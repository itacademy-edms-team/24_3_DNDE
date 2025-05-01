import React, { ChangeEvent, useEffect, useState } from 'react';
import { useAppDispatch, useAppSelector } from '../../hooks';

import { selectCharacterName } from '../../store/selectors/sheet1Selectors';
import { selectCantrips } from '../../store/selectors/sheet3Selectors';

import { updateName } from '../../store/sheet1Slice';
import {
  updateSpellCastingAbility,
  addCantrip,
  updateCantrip,
  deleteCantrip,
  updateCantripName,
  updateCantripSchool,
  updateCantripCantripProgression,
  updateCantripIsRitual,
  updateCantripCastingTime,
  updateCantripRange,
  updateCantripTarget,
  updateCantripComponentsV,
  updateCantripComponentsS,
  updateCantripComponentsM,
  updateCantripComponentsDescription,
  updateCantripIsConcentration,
  updateCantripDuration,
  updateCantripSpellCastingAbility,
  updateCantripInnate,
  updateCantripOutput,
  updateCantripAttackType,
  updateCantripAttackDamage1Dice,
  updateCantripAttackDamage1Type,
  updateCantripAttackDamage2Dice,
  updateCantripAttackDamage2Type,
  updateCantripHealingDice,
  updateCantripIsAbilityModIncluded,
  updateCantripSavingThrowAbility,
  updateCantripSavingThrowEffect,
  updateCantripHigherLevelCastDiceAmount,
  updateCantripHigherLevelCastDiceType,
  updateCantripHigherLevelCastBonus,
  updateCantripIncludeSpellDescriptionInAttack,
  updateCantripDescription,
  updateCantripAtHigherLevels,
  updateCantripClass,
  updateCantripType
} from '../../store/sheet3Slice';

import { v4 as uuidv4 } from 'uuid';

import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Form from 'react-bootstrap/Form';
import Card from 'react-bootstrap/Card';
import Container from 'react-bootstrap/Container';
import Collapse from 'react-bootstrap/Collapse';
import InputGroup from 'react-bootstrap/InputGroup';
import { FaCog, FaTrash, FaLock, FaLockOpen, FaPlus, FaInfoCircle } from 'react-icons/fa';

import CharacterSpellcastingCantripsSVG from './assets/CharacterSpellcastingCantrips.svg';

import './CantripItem.css';

import { Spell, SpellAbilityType, CantripCardPropsType, RootState, SpellItemToggleType, SpellSchool } from '../../types/state';

const CantripItem: React.FC<CantripCardPropsType> = ({
  cantrip,
  isEditMode,
  onDelete
}) => {
  const dispatch = useAppDispatch();

  const [isInfoExpanded, setInfoExpanded] = useState(false);
  const [isEditExpanded, setEditExpanded] = useState(false);

  const toggleIsInfoExpanded = () => {
    setInfoExpanded(!isInfoExpanded);
  }
  const toggleIsEditExpanded = () => {
    setEditExpanded(!isEditExpanded);
  }

  const handleNameChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripName({ id: cantrip.id, value: e.target.value }));
  };
  const handleSchoolChange = (e: ChangeEvent<HTMLSelectElement>) => {
    dispatch(updateCantripSchool({ id: cantrip.id, value: e.target.value as SpellSchool }));
  };
  const handleCantripProgressionChange = (e: ChangeEvent<HTMLSelectElement>) => {
    dispatch(updateCantripCantripProgression({ id: cantrip.id, value: e.target.value as any }));
  };
  const handleIsRitualChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripIsRitual({ id: cantrip.id, value: e.target.checked }));
  };
  const handleCastingTimeChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripCastingTime({ id: cantrip.id, value: e.target.value }));
  };
  const handleRangeChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripRange({ id: cantrip.id, value: e.target.value }));
  };
  const handleTargetChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripTarget({ id: cantrip.id, value: e.target.value }));
  };
  const handleComponentsVChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripComponentsV({ id: cantrip.id, value: e.target.checked }));
  };
  const handleComponentsSChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripComponentsS({ id: cantrip.id, value: e.target.checked }));
  };
  const handleComponentsMChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripComponentsM({ id: cantrip.id, value: e.target.checked }));
  };
  const handleComponentsDescriptionChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripComponentsDescription({ id: cantrip.id, value: e.target.value }));
  };
  const handleIsConcentrationChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripIsConcentration({ id: cantrip.id, value: e.target.checked }));
  };
  const handleDurationChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripDuration({ id: cantrip.id, value: e.target.value }));
  };
  const handleSpellCastingAbilityChange = (e: ChangeEvent<HTMLSelectElement>) => {
    dispatch(updateCantripSpellCastingAbility({ id: cantrip.id, value: e.target.value as any }));
  };
  const handleInnateChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripInnate({ id: cantrip.id, value: e.target.value }));
  };
  const handleOutputChange = (e: ChangeEvent<HTMLSelectElement>) => {
    dispatch(updateCantripOutput({ id: cantrip.id, value: e.target.value as any }));
  };
  const handleAttackTypeChange = (e: ChangeEvent<HTMLSelectElement>) => {
    dispatch(updateCantripAttackType({ id: cantrip.id, value: e.target.value as any }));
  };
  const handleAttackDamage1DiceChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripAttackDamage1Dice({ id: cantrip.id, value: e.target.value }));
  };
  const handleAttackDamage1TypeChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripAttackDamage1Type({ id: cantrip.id, value: e.target.value }));
  };
  const handleAttackDamage2DiceChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripAttackDamage2Dice({ id: cantrip.id, value: e.target.value }));
  };
  const handleAttackDamage2TypeChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripAttackDamage2Type({ id: cantrip.id, value: e.target.value }));
  };
  const handleHealingDiceChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripHealingDice({ id: cantrip.id, value: e.target.value }));
  };
  const handleIsAbilityModIncludedChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripIsAbilityModIncluded({ id: cantrip.id, value: e.target.checked }));
  };
  const handleSavingThrowAbilityChange = (e: ChangeEvent<HTMLSelectElement>) => {
    dispatch(updateCantripSavingThrowAbility({ id: cantrip.id, value: e.target.value as any }));
  };
  const handleSavingThrowEffectChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripSavingThrowEffect({ id: cantrip.id, value: e.target.value }));
  };
  const handleHigherLevelCastDiceAmountChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripHigherLevelCastDiceAmount({ id: cantrip.id, value: parseInt(e.target.value) || 0 }));
  };
  const handleHigherLevelCastDiceTypeChange = (e: ChangeEvent<HTMLSelectElement>) => {
    dispatch(updateCantripHigherLevelCastDiceType({ id: cantrip.id, value: e.target.value as any }));
  };
  const handleHigherLevelCastBonusChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripHigherLevelCastBonus({ id: cantrip.id, value: parseInt(e.target.value) || 0 }));
  };
  const handleIncludeSpellDescriptionInAttackChange = (e: ChangeEvent<HTMLSelectElement>) => {
    dispatch(updateCantripIncludeSpellDescriptionInAttack({ id: cantrip.id, value: e.target.value as any }));
  };
  const handleDescriptionChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripDescription({ id: cantrip.id, value: e.target.value }));
  };
  const handleAtHigherLevelsChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripAtHigherLevels({ id: cantrip.id, value: e.target.value }));
  };
  const handleClassChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripClass({ id: cantrip.id, value: e.target.value }));
  };
  const handleTypeChange = (e: ChangeEvent<HTMLInputElement>) => {
    dispatch(updateCantripType({ id: cantrip.id, value: e.target.value }));
  };

  const toggleGroup = isEditMode ? (
    <div
      className="position-absolute top-50 translate-middle-y d-flex d-block gap-1"
      style={{ right: "0.5rem" }}
    >
      <FaCog
        onClick={toggleIsEditExpanded}
        className="cantrip-icon"
      />
      <FaInfoCircle
        onClick={toggleIsInfoExpanded}
        className="cantrip-icon"
      />
    </div>
  ) : (
    <FaTrash
      aria-label={`Удалить заговор ${cantrip.name}`}
      onClick={onDelete}
      className="cantrip-icon position-absolute top-50 translate-middle-y"
      style={{ right: "0.5rem" }}
    />
  );

  const getVSMString = (cantrip: Spell) => {
    const v = cantrip.components.v.isIncluded ? 'В' : '';
    const s = cantrip.components.s.isIncluded ? 'С' : '';
    const m = cantrip.components.m.isIncluded ? 'М' : '';
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
    transmutation: 'Трансмутация'
  };

  return (
    <Container className="px-0">
      <Card>
        <Card.Header className="cantrip-header p-1 d-flex flex-row">
          <div className="flex-grow-1 position-relative">
            <Form.Control
              id={`cantrip-${cantrip.id}-name`}
              value={cantrip.name}
              readOnly
              aria-label={`Название заговора ${cantrip.name}`}
            />
            {toggleGroup}
          </div>
          <Form.Control
            id={`cantrip-${cantrip.id}-VSM`}
            value={getVSMString(cantrip)}
            readOnly
            style={{ width: "4rem" }}
          />
        </Card.Header>
      </Card>
      <Collapse in={isInfoExpanded}>
        <Card.Body className="p-1">
          <div className="d-flex flex-column gap-1">
            <div className="fst-italic" style={{ color: "crimson" }}>{cantrip.name}</div>
            <div><span className="fw-bold">Школа:</span> {schoolTranslations[cantrip.school]}</div>
            <div><span className="fw-bold">Время каста:</span> {cantrip.castingTime}</div>
            <div><span className="fw-bold">Дальность:</span> {cantrip.range}</div>
            <div><span className="fw-bold">Цель:</span> {cantrip.target}</div>
            <div><span className="fw-bold">Компоненты:</span> {getVSMString(cantrip)}</div>
            <div><span className="fw-bold">Длительность:</span> {cantrip.duration}</div>
            <div><span className="fw-bold">Описание:</span> {cantrip.description}</div>
          </div>
        </Card.Body>
      </Collapse>
      <Collapse in={isEditExpanded}>
        <Card.Body className="p-1">
          <div className="d-flex flex-column gap-1 text-nowrap">
            <Form.Group
              controlId={`cantrip-${cantrip.id}-name-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Название</Form.Label>
              <Form.Control
                size="sm"
                value={cantrip.name}
                onChange={handleNameChange}
                aria-label={`Название заговора ${cantrip.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-school-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Школа</Form.Label>
              <Form.Select
                size="sm"
                value={cantrip.school}
                onChange={handleSchoolChange}
                aria-label={`Школа заговора ${cantrip.name}`}
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
            <Form.Group
              controlId={`cantrip-${cantrip.id}-isRitual-edit`}
              className="d-flex flex-row gap-1"
            >
              <Form.Label className="m-0 fw-bold">Ритуальное</Form.Label>
              <Form.Check
                type="checkbox"
                inline
                checked={cantrip.isRitual}
                onChange={handleIsRitualChange}
                aria-label={`Ритуал заговора ${cantrip.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-castingTime-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Время каста</Form.Label>
              <Form.Control
                size="sm"
                value={cantrip.castingTime}
                onChange={handleCastingTimeChange}
                aria-label={`Время каста заговора ${cantrip.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-range-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Дальность</Form.Label>
              <Form.Control
                size="sm"
                value={cantrip.range}
                onChange={handleRangeChange}
                aria-label={`Дальность заговора ${cantrip.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-target-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Цель</Form.Label>
              <Form.Control
                size="sm"
                value={cantrip.target}
                onChange={handleTargetChange}
                aria-label={`Цель заговора ${cantrip.name}`}
              />
            </Form.Group>
            <div className="d-flex flex-row gap-3 align-items-center">
              <div><span className="fw-bold">Компоненты</span></div>
              <Form.Group
                controlId={`cantrip-${cantrip.id}-components-v-edit`}
                className="d-flex flex-row gap-1 justify-content-center align-items-center"
              >
                <Form.Label className="m-0 fw-bold">В</Form.Label>
                <Form.Check
                  type="checkbox"
                  inline
                  checked={cantrip.components.v.isIncluded}
                  onChange={handleComponentsVChange}
                  aria-label={`Вербальный компонент заговора ${cantrip.name}`}
                  className="m-0"
                />
              </Form.Group>
              <Form.Group
                controlId={`cantrip-${cantrip.id}-components-s-edit`}
                className="d-flex flex-row gap-1 justify-content-center align-items-center"
              >
                <Form.Label className="m-0 fw-bold">С</Form.Label>
                <Form.Check
                  type="checkbox"
                  inline
                  checked={cantrip.components.s.isIncluded}
                  onChange={handleComponentsSChange}
                  aria-label={`Соматический компонент заговора ${cantrip.name}`}
                  className="m-0"
                />
              </Form.Group>
              <Form.Group
                controlId={`cantrip-${cantrip.id}-components-m-edit`}
                className="d-flex flex-row gap-1 justify-content-center align-items-center"
              >
                <Form.Label className="m-0 fw-bold">М</Form.Label>
                <Form.Check
                  type="checkbox"
                  inline
                  checked={cantrip.components.m.isIncluded}
                  onChange={handleComponentsMChange}
                  aria-label={`Материальный компонент заговора ${cantrip.name}`}
                  className="m-0"
                />
              </Form.Group>
            </div>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-componentsDescription-edit`}
              className="d-flex flex-column gap-1 justify-content-center"
            >
              <Form.Label className="m-0 fw-bold">Описание компонентов</Form.Label>
              <Form.Control
                size="sm"
                as="textarea"
                rows={1}
                value={cantrip.componentsDescription}
                onChange={handleComponentsDescriptionChange}
                aria-label={`Описание компонентов заговора ${cantrip.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-isConcentration-edit`}
              className="d-flex flex-row gap-1"
            >
              <Form.Label className="m-0 fw-bold">Требуется концентрация</Form.Label>
              <Form.Check
                type="checkbox"
                checked={cantrip.isConcentration}
                onChange={handleIsConcentrationChange}
                aria-label={`Концентрация заговора ${cantrip.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-duration-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Длительность</Form.Label>
              <Form.Control
                size="sm"
                value={cantrip.duration}
                onChange={handleDurationChange}
                aria-label={`Длительность заговора ${cantrip.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-spellCastingAbility-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Способность</Form.Label>
              <Form.Select
                size="sm"
                value={cantrip.spellCastingAbility}
                onChange={handleSpellCastingAbilityChange}
                aria-label={`Способность для каста заговора ${cantrip.name}`}
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
              controlId={`cantrip-${cantrip.id}-innate-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Кол-во использований</Form.Label>
              <Form.Control
                size="sm"
                value={cantrip.innate}
                onChange={handleInnateChange}
                placeholder="1/день"
                aria-label={`Врождённое заговора ${cantrip.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-output-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Вывод</Form.Label>
              <Form.Select
                size="sm"
                value={cantrip.output}
                onChange={handleOutputChange}
                aria-label={`Вывод заговора ${cantrip.name}`}
              >
                <option value="spellcard">Карта заклинания</option>
                <option value="attack">Атака</option>
              </Form.Select>
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-attack-attackType-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Тип атаки</Form.Label>
              <Form.Select
                size="sm"
                value={cantrip.attack.attackType}
                onChange={handleAttackTypeChange}
                aria-label={`Тип атаки заговора ${cantrip.name}`}
              >
                <option value="ranged">Дальний</option>
                <option value="mele">Ближний</option>
              </Form.Select>
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-attack-damage1-dice-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Урон 1: Кости</Form.Label>
              <Form.Control
                size="sm"
                value={cantrip.attack.damage1.dice}
                onChange={handleAttackDamage1DiceChange}
                aria-label={`Кости урона 1 заговора ${cantrip.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-attack-damage1-type-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Урон 1: Тип</Form.Label>
              <Form.Control
                size="sm"
                value={cantrip.attack.damage1.type}
                onChange={handleAttackDamage1TypeChange}
                aria-label={`Тип урона 1 заговора ${cantrip.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-attack-damage2-dice-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Урон 2: Кости</Form.Label>
              <Form.Control
                size="sm"
                value={cantrip.attack.damage2.dice}
                onChange={handleAttackDamage2DiceChange}
                aria-label={`Кости урона 2 заговора ${cantrip.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-attack-damage2-type-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Урон 2: Тип</Form.Label>
              <Form.Control
                size="sm"
                value={cantrip.attack.damage2.type}
                onChange={handleAttackDamage2TypeChange}
                aria-label={`Тип урона 2 заговора ${cantrip.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-healingDice-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Лечащие кости</Form.Label>
              <Form.Control
                size="sm"
                value={cantrip.healingDice}
                onChange={handleHealingDiceChange}
                aria-label={`Лечащие кости заговора ${cantrip.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-isAbilityModIncluded-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold text-wrap">Учитывать модификатор способности</Form.Label>
              <Form.Check
                type="checkbox"
                checked={cantrip.isAbilityModIncluded}
                onChange={handleIsAbilityModIncludedChange}
                aria-label={`Модификатор способности заговора ${cantrip.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-cantripProgression-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Прогрессия</Form.Label>
              <Form.Select
                size="sm"
                value={cantrip.spellProgression}
                onChange={handleCantripProgressionChange}
                aria-label={`Прогрессия заговора ${cantrip.name}`}
              >
                <option value="none">Нет</option>
                <option value="cantripBeam">Луч</option>
                <option value="cantripDice">Кости</option>
              </Form.Select>
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-savingThrow-ability-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Спасбросок: Способность</Form.Label>
              <Form.Select
                size="sm"
                value={cantrip.savingThrow.ability}
                onChange={handleSavingThrowAbilityChange}
                aria-label={`Способность спасброска заговора ${cantrip.name}`}
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
              controlId={`cantrip-${cantrip.id}-savingThrow-effect-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Спасбросок: Эффект</Form.Label>
              <Form.Control
                size="sm"
                value={cantrip.savingThrow.effect}
                onChange={handleSavingThrowEffectChange}
                aria-label={`Эффект спасброска заговора ${cantrip.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-higherLevelCast-diceAmount-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold text-wrap">Каст на высоком уровне: Кол-во костей</Form.Label>
              <Form.Control
                size="sm"
                type="number"
                value={cantrip.higherLevelCast.diceAmount}
                onChange={handleHigherLevelCastDiceAmountChange}
                aria-label={`Количество костей для каста на высоком уровне заговора ${cantrip.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-higherLevelCast-diceType-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold text-wrap">Каст на высоком уровне: Тип костей</Form.Label>
              <Form.Select
                size="sm"
                value={cantrip.higherLevelCast.diceType}
                onChange={handleHigherLevelCastDiceTypeChange}
                aria-label={`Тип костей для каста на высоком уровне заговора ${cantrip.name}`}
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
              controlId={`cantrip-${cantrip.id}-higherLevelCast-bonus-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Каст на высоком уровне: Бонус</Form.Label>
              <Form.Control
                size="sm"
                type="number"
                value={cantrip.higherLevelCast.bonus}
                onChange={handleHigherLevelCastBonusChange}
                aria-label={`Бонус для каста на высоком уровне заговора ${cantrip.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-includeSpellDescriptionInAttack-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Описание в атаке</Form.Label>
              <Form.Select
                size="sm"
                value={cantrip.includeSpellDescriptionInAttack}
                onChange={handleIncludeSpellDescriptionInAttackChange}
                aria-label={`Описание в атаке заговора ${cantrip.name}`}
              >
                <option value="off">Выкл</option>
                <option value="partial">Частично</option>
                <option value="on">Вкл</option>
              </Form.Select>
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-description-edit`}
              className="d-flex flex-column gap-1"
            >
              <Form.Label className="m-0 fw-bold">Описание</Form.Label>
              <Form.Control
                size="sm"
                as="textarea"
                rows={3}
                value={cantrip.description}
                onChange={handleDescriptionChange}
                aria-label={`Описание заговора ${cantrip.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-atHigherLevels-edit`}
              className="d-flex flex-column gap-1"
            >
              <Form.Label className="m-0 fw-bold">На высоких уровнях</Form.Label>
              <Form.Control
                size="sm"
                as="textarea"
                rows={3}
                value={cantrip.atHigherLevels}
                onChange={handleAtHigherLevelsChange}
                aria-label={`На высоких уровнях заговора ${cantrip.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-class-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Класс</Form.Label>
              <Form.Control
                size="sm"
                value={cantrip.class}
                onChange={handleClassChange}
                aria-label={`Класс заговора ${cantrip.name}`}
              />
            </Form.Group>
            <Form.Group
              controlId={`cantrip-${cantrip.id}-type-edit`}
              className="d-flex flex-row gap-1 justify-content-center align-items-center"
            >
              <Form.Label className="m-0 fw-bold">Тип</Form.Label>
              <Form.Control
                size="sm"
                value={cantrip.type}
                onChange={handleTypeChange}
                aria-label={`Тип заговора ${cantrip.name}`}
              />
            </Form.Group>
          </div>
        </Card.Body>
      </Collapse>
    </Container>
  );
}

const CantripComponent: React.FC = () => {
  const dispatch = useAppDispatch();

  const cantrips = useAppSelector(selectCantrips);

  const [isEditMode, setEditMode] = useState(true); // false -> deletionMode
  const toggleEditMode = () => setEditMode(prev => !prev);

  const AddNewCantrip = () => {
    const newCantrip: Spell = {
      id: uuidv4(),
      name: "Новый заговор",
      school: "abjuration",
      isRitual: false,
      castingTime: "",
      range: "",
      target: "",
      components: {
        v: { isIncluded: true },
        s: { isIncluded: true },
        m: { isIncluded: true }
      },
      componentsDescription: "",
      isConcentration: false,
      duration: "",
      spellCastingAbility: "none",
      innate: "",
      output: "spellcard",
      attack: {
        attackType: "ranged",
        damage1: {
          dice: "",
          type: ""
        },
        damage2: {
          dice: "",
          type: ""
        }
      },
      healingDice: "",
      isAbilityModIncluded: false, // Add ability mod to attack or no
      savingThrow: {
        ability: "none",
        effect: ""
      },
      cantripProgression: "none",
      higherLevelCast: {
        diceType: "none",
        diceAmount: 0,
        bonus: 0
      },
      includeSpellDescriptionInAttack: "off",
      description: "",
      atHigherLevels: "",
      class: "",
      type: "",
      isPrepared: true
    };
    dispatch(addCantrip(newCantrip));
  }

  const DeleteCantripById = (cantripId: string) => {
    dispatch(deleteCantrip(cantripId));
  }

  return (
    <Container>
      <Card className="border-0">
        <Card.Img src={CharacterSpellcastingCantripsSVG} />
        <Card.ImgOverlay className="h-100 d-flex justify-content-center align-items-center">
          <div className="fw-bold text-wrap  text-uppercase text-center">
            Заговоры
          </div>
        </Card.ImgOverlay>
      </Card>
      <Container fluid className="px-0 pt-1">
        <Container
          className="px-0 d-flex flex-column gap-2">
          {cantrips.length === 0 ? (
            <div className="text-wrap fw-lighter">
              Нету заговоров
            </div>
          ) : (
            cantrips.map((c: Cantrip) => (
              <CantripItem
                key={c.id}
                cantrip={c}
                isEditMode={isEditMode}
                onDelete={() => DeleteCantripById(c.id)}
              />
            ))
          )}
        </Container>
      </Container>
      <Container className="mt-2 d-flex justify-content-between">
        {isEditMode ? (
          <>
            <FaPlus
              onClick={AddNewCantrip}
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

export default CantripComponent;