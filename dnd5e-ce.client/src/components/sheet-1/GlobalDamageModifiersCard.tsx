import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';
import Table from 'react-bootstrap/Table';
import Collapse from 'react-bootstrap/Collapse';

import { FaCog, FaTrash, FaLock, FaLockOpen, FaPlus } from 'react-icons/fa';

import { v4 as uuidv4 } from 'uuid';
import { useState, Fragment, useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '../../hooks';
import {
  addGlobalDamageModifier, updateGlobalDamageModifier, deleteGlobalDamageModifier
} from '../../store/sheet1Slice';
import { RootState, AttacksCardAttackEditableRowPropsType, Attack, AbilityType, AttackAbilityType, DCAbilityType, DamageAbilityType, AttacksCardGlobalDamageModifierEditableRowPropsType, GlobalDamageModifier } from '../../types/state';

const GlobalDamageModifierEditableRow: React.FC<AttacksCardGlobalDamageModifierEditableRowPropsType> = ({
  globalDamageModifier,
  isEditMode,
  onDelete
}) => {

  const dispatch = useAppDispatch();

  const [isExpanded, setExpanded] = useState(false);

  // Сворачиваем форму при смене режима
  useEffect(() => {
    if (!isEditMode) {
      setExpanded(false);
    }
  }, [isEditMode]);

  const toggleExpand = () => setExpanded((prev) => !prev);

  // Обработчик обновления модификатора
  const updateModifierField = (updates: Partial<GlobalDamageModifier>) => {
    dispatch(updateGlobalDamageModifier({ id: globalDamageModifier.id, ...updates }));
  };

  const handleIsIncludedChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateModifierField({ isIncluded: event.target.checked });
  };

  const handleNameChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateModifierField({ name: event.target.value });
  };

  const handleDamageDiceChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateModifierField({ damageDice: event.target.value });
  };

  const handleCriticalDamageDiceChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateModifierField({ criticalDamageDice: event.target.value });
  };

  const handleTypeChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateModifierField({ type: event.target.value });
  };

  const displayDamage = () => {
    const damages = [];
    if (globalDamageModifier.damageDice) {
      damages.push(globalDamageModifier.damageDice);
    }
    if (globalDamageModifier.criticalDamageDice) {
      damages.push(`(+ ${globalDamageModifier.criticalDamageDice} при крите)`);
    }
    damages.push(`[${globalDamageModifier.type}]`);
    return damages.join(" ");
  }

  return (
    <Fragment>
      <Row className="pt-3 align-items-center">
        <Col md={5} className="text-truncate">
          {globalDamageModifier.name}
        </Col>
        <Col md={6}>
          {displayDamage()}
        </Col>
        <Col md={1} className="text-center">
          {isEditMode ? (
            <FaCog
              onClick={toggleExpand}
              style={{ cursor: 'pointer'}}
              aria-label={`Редактировать глобальный модификатор атаки ${globalDamageModifier.name}`}
            />
          ) : (
            <FaTrash
              onClick={onDelete}
              style={{ cursor: 'pointer'}}
              aria-label={`Удалить глобальный модификатор атаки ${globalDamageModifier.name}`}
            />
          )}
        </Col>
      </Row>
      <Collapse in={isExpanded}>
        <Row className="p-3">
          <Form.Group controlId={`globalDamageModifier-${globalDamageModifier.id}-editName`} className="mb-2">
            <Form.Label className="fw-bold">Название</Form.Label>
            <Form.Control
              type="text"
              value={globalDamageModifier.name}
              onChange={handleNameChange}
              placeholder="Например, Урон от Ярости"
              aria-label="Название модификатора"
            />
          </Form.Group>
          <Form.Group controlId={`globalDamageModifier-${globalDamageModifier.id}-editDamageDice`} className="mb-2">
            <Form.Label className="fw-bold">Кубик урона</Form.Label>
            <Form.Control
              type="text"
              value={globalDamageModifier.damageDice}
              onChange={handleDamageDiceChange}
              placeholder="Например, 1d6"
              aria-label="Кубики урона модификатора"
            />
          </Form.Group>
          <Form.Group controlId={`globalDamageModifier-${globalDamageModifier.id}-editCriticalDamageDice`} className="mb-2">
            <Form.Label className="fw-bold">
              Кубики критического урона
            </Form.Label>
            <Form.Control
              type="text"
              value={globalDamageModifier.criticalDamageDice}
              onChange={handleCriticalDamageDiceChange}
              placeholder="Например, 1d4"
              aria-label="Кубики критического урона модификатора"
            />
          </Form.Group>
          <Form.Group controlId={`globalDamageModifier-${globalDamageModifier.id}-editType`} className="mb-2">
            <Form.Label className="fw-bold">Тип</Form.Label>
            <Form.Control
              type="text"
              value={globalDamageModifier.type}
              onChange={handleTypeChange}
              placeholder="Например, Ярость"
              aria-label="Тип модификатора"
            />
          </Form.Group>
        </Row>
      </Collapse>
    </Fragment>
  );
}

const GlobalDamageModifiersCard: React.FC = () => {
  const dispatch = useAppDispatch();
  const globalDamageModifiers = useAppSelector((state: RootState) => state.sheet1.globalDamageModifiers);

  const [isEditMode, setEditMode] = useState(true); // false -> deletionMode
  const toggleEditMode = () => setEditMode(prev => !prev);

  const addNewModifier = () => {
    const newModifier: GlobalDamageModifier = {
      id: uuidv4(),
      name: "Урон от Новый Модификатор",
      damageDice: "", // 1d6, 1d4, ...
      criticalDamageDice: "", // 1d6, 1d4, ...
      type: "Новый Модификатор",
      isIncluded: false
    }
    dispatch(addGlobalDamageModifier(newModifier));
  }

  const deleteGlobalDamageModifierById = (id: string) => {
    return () => dispatch(deleteGlobalDamageModifier(id));
  };

  return (
    <>
      <Container className="border border-rounded">
        <Container fluid={true} className="p-1 m-0">
          <Row className="fw-bold">
            <Col md={5}>Имя</Col>
            <Col md={6}>Урон</Col>
            <Col md={1}></Col>
          </Row>
          <Row>
            {globalDamageModifiers.length > 0 ? (
              globalDamageModifiers.map((mod) => (
                <GlobalDamageModifierEditableRow
                  key={mod.id}
                  globalDamageModifier={mod}
                  isEditMode={isEditMode}
                  onDelete={deleteGlobalDamageModifierById(mod.id)}
                />
              ))
            ) : (
              <Row className="py-2">
                <Col className="text-center text-muted">Нет модификаторов</Col>
              </Row>
            )}
          </Row>
          <Row className="mt-3">
            <Col className="d-flex justify-content-center gap-3">
              {isEditMode ? (
                <>
                  <FaPlus
                    onClick={addNewModifier}
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
          <div className="p-0 m-0 mt-2 text-center">
            <span className="text-uppercase fw-bold">Глобальные модификаторы урона</span>
          </div>
        </Container>
      </Container>
    </>
  );
}

export default GlobalDamageModifiersCard;