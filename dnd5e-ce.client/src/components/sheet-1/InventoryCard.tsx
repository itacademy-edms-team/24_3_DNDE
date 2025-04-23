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
import { updateInventoryGold, addInventoryItem, updateInventoryItem, deleteInventoryItem } from '../../store/sheet1Slice';
import { RootState, AttacksCardAttackEditableRowPropsType, Attack, AbilityType, AttackAbilityType, DCAbilityType, DamageAbilityType, AttacksCardGlobalDamageModifierEditableRowPropsType, GlobalDamageModifier, InventoryGold, InventoryItemEditableRowPropsType, InventoryItem } from '../../types/state';


const GoldCard: React.FC = () => {
  const dispatch = useAppDispatch();

  const gold = useAppSelector((state: RootState) => state.sheet1.inventory.gold);

  const handleGoldChange = (updates: Partial<InventoryGold>) => {
    dispatch(updateInventoryGold({ ...updates }));
  };

  const handleGoldCPChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(e.target.value);
    if (!isNaN(newValue)) {
      handleGoldChange({ cp: newValue });
    }
  };

  const handleGoldSPChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(e.target.value);
    if (!isNaN(newValue)) {
      handleGoldChange({ sp: newValue });
    }
  };

  const handleGoldEPChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(e.target.value);
    if (!isNaN(newValue)) {
      handleGoldChange({ ep: newValue });
    }
  };

  const handleGoldGPChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(e.target.value);
    if (!isNaN(newValue)) {
      handleGoldChange({ gp: newValue });
    }
  };

  const handleGoldPPChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(e.target.value);
    if (!isNaN(newValue)) {
      handleGoldChange({ pp: newValue });
    }
  };

  return (
    <>
      <Container className="p-2">
        <Row>
          <Col>
            <Form.Group controlId="characterGold-cp" className="text-center">
              <Form.Control
                value={gold.cp}
                onChange={handleGoldCPChange}
                className="p-0 m-0 text-center"
              />
              <Form.Label className="fw-bold">ММ</Form.Label>
            </Form.Group>
          </Col>
          <Col>
            <Form.Group controlId="characterGold-sp" className="text-center">
              <Form.Control
                value={gold.sp}
                onChange={handleGoldSPChange}
                className="p-0 m-0 text-center"
              />
              <Form.Label className="fw-bold">СМ</Form.Label>
            </Form.Group>
          </Col>
          <Col>
            <Form.Group controlId="characterGold-gp" className="text-center">
              <Form.Control
                value={gold.gp}
                onChange={handleGoldGPChange}
                className="p-0 m-0 text-center"
              />
              <Form.Label className="fw-bold">ЗМ</Form.Label>
            </Form.Group>
          </Col>
          <Col>
            <Form.Group controlId="characterGold-ep" className="text-center">
              <Form.Control
                value={gold.ep}
                onChange={handleGoldEPChange}
                className="p-0 m-0 text-center"
              />
              <Form.Label className="fw-bold">ЭМ</Form.Label>
            </Form.Group>
          </Col>
          <Col>
            <Form.Group controlId="characterGold-pp" className="text-center">
              <Form.Control
                value={gold.pp}
                onChange={handleGoldPPChange}
                className="p-0 m-0 text-center"
              />
              <Form.Label className="fw-bold">ПМ</Form.Label>
            </Form.Group>
          </Col>
        </Row>
      </Container>
    </>
  );
}

const InventoryItemEditableRow: React.FC<InventoryItemEditableRowPropsType> = ({
  inventoryItem,
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

  // Обработчик обновления предмета
  const updateItemField = (updates: Partial<InventoryItem>) => {
    dispatch(updateInventoryItem({ id: inventoryItem.id, ...updates }));
  };

  // Обработчики изменений
  const handleIsEquippedChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateItemField({ isEquipped: event.target.checked });
  };

  const handleIsUsedAsResourceChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateItemField({ isUsedAsResource: event.target.checked });
  };

  const handleIsHasAnAttackChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateItemField({ isHasAnAttack: event.target.checked });
  };

  const handleNameChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateItemField({ name: event.target.value });
  };

  const handleAmountChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = parseFloat(event.target.value);
    if (!isNaN(value) && value > 0) {
      updateItemField({ amount: Math.floor(value) });
    } else {
      updateItemField({ amount: 1 });
    }
  };

  const handleWeightChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = parseFloat(event.target.value);
    if (!isNaN(value) && value > 0) {
      updateItemField({ weight: value });
    } else {
      updateItemField({ weight: 0.1 });
    }
  };

  const handlePropChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateItemField({ prop: event.target.value });
  };

  const handleDescriptionChange = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
    updateItemField({ description: event.target.value });
  };

  return (
    <Fragment>
      <Row className="pt-3 align-items-center">
        <Col md={1} className="p-0 m-0 text-center">
          <Form.Check
            type="checkbox"
            id={`inventoryItem-${inventoryItem.id}-isEquipped`}
            checked={inventoryItem.isEquipped}
            onChange={handleIsEquippedChange}
            aria-label={`Надеть предмет ${inventoryItem.name}`}
          />
        </Col>
        <Col md={2} className="p-0 m-0 text-center">
          <Form.Control
            type="number"
            value={inventoryItem.amount}
            onChange={handleAmountChange}
            min="0"
            step="1"
            aria-label={`Количество предмета ${inventoryItem.name}`}
            className="p-0 m-0 h-100"
          />
        </Col>
        <Col md={6} className="p-0 m-0 text-truncate">
          <Form.Control
            type="text"
            value={inventoryItem.name}
            onChange={handleNameChange}
            aria-label={`Название предмета ${inventoryItem.name}`}
            className="p-0 m-0 h-100"
          />
        </Col>
        <Col md={2} className="p-0 m-0 text-center">
          <Form.Control
            type="number"
            value={inventoryItem.weight}
            onChange={handleWeightChange}
            min="0.1"
            step="0.1"
            aria-label={`Вес предмета ${inventoryItem.name}`}
            className="p-0 m-0 h-100"
          />
        </Col>
        <Col md={1} className="text-center">
          {isEditMode ? (
            <FaCog
              onClick={toggleExpand}
              style={{ cursor: 'pointer' }}
              aria-label={`Редактировать предмет ${inventoryItem.name}`}
            />
          ) : (
            <FaTrash
              onClick={onDelete}
              style={{ cursor: 'pointer' }}
                aria-label={`Удалить предмет ${inventoryItem.name}`}
            />
          )}
        </Col>
      </Row>
      <Collapse in={isExpanded}>
        <Container className="p-3">
          <Form.Group controlId={`item-${inventoryItem.id}-isEquipped`}>
            <Form.Check
              id={`inventoryItem-${inventoryItem.id}-isEquipped`}
              inline
              checked={inventoryItem.isEquipped}
              onChange={handleIsEquippedChange}
            />
            <Form.Label className="fw-bold">Надето</Form.Label>
          </Form.Group>
          <Form.Group>
            <Form.Check
              id={`inventoryItem-${inventoryItem.id}-isUsedAsResource`}
              inline
              checked={inventoryItem.isUsedAsResource}
              onChange={handleIsUsedAsResourceChange}
            />
            <Form.Label className="fw-bold">Используется как ресурс</Form.Label>
          </Form.Group>
          <Form.Group>
            <Form.Check
              id={`inventoryItem-${inventoryItem.id}-isHasAnAttack`}
              inline
              checked={inventoryItem.isHasAnAttack}
              onChange={handleIsHasAnAttackChange}
            />
            <Form.Label className="fw-bold">Есть атака</Form.Label>
          </Form.Group>
          <Form.Group controlId={`item-${inventoryItem.id}-editProp`} className="mb-2">
            <Form.Label className="fw-bold">Свойство</Form.Label>
            <Form.Control
              type="text"
              value={inventoryItem.prop}
              onChange={handlePropChange}
              placeholder="Например, Лёгкое"
              aria-label="Свойство предмета"
            />
          </Form.Group>
          <Form.Group controlId={`item-${inventoryItem.id}-editDescription`} className="mb-2">
            <Form.Label className="fw-bold">Описание</Form.Label>
            <Form.Control
              as="textarea"
              rows={3}
              value={inventoryItem.description}
              onChange={handleDescriptionChange}
              placeholder="Например, Магический меч с огненным уроном"
              aria-label="Описание предмета"
            />
          </Form.Group>
        </Container>
      </Collapse>
    </Fragment>
  );
}

const InventoryItemsCard: React.FC = () => {
  const dispatch = useAppDispatch();

  const items = useAppSelector((state: RootState) => state.sheet1.inventory.items);

  const [isEditMode, setEditMode] = useState(true); // false -> deletionMode
  const toggleEditMode = () => setEditMode(prev => !prev);

  const addNewItem = () => {
    const newItem = {
      id: uuidv4(),
      amount: 1,
      name: 'Новый предмет',
      weight: 0,
      isEquipped: false,
      isUsedAsResource: false,
      isHasAnAttack: false,
      prop: '',
      description: ''
    }
    dispatch(addInventoryItem(newItem));
  }

  const deleteItemById = (id: string) => {
    return () => dispatch(deleteInventoryItem(id));
  };

  const calculateTotalWeight = (items: InventoryItem[]) => {
    return items.reduce((acc, item) => acc + item.weight * item.amount, 0).toFixed(2);
  };

  return (
    <>
      <Container>
        {items.length > 0 ? (
          items.map((item) => (
            <InventoryItemEditableRow
              key={item.id}
              inventoryItem={item}
              isEditMode={isEditMode}
              onDelete={deleteItemById(item.id)}
            />
          ))
        ) : (
          <Row className="py-2">
            <Col className="text-center text-muted">Нет предметов</Col>
          </Row>
        )}

        <Container>
          <Row className="py-2">
            <Col className="text-center fw-bold">Общий вес</Col>
          </Row>
          <Row>
            <Col className="text-center">{calculateTotalWeight(items)}</Col>
          </Row>
        </Container>

        <Container className="mt-3">
          <Col className="d-flex justify-content-center gap-3">
            {isEditMode ? (
              <>
                <FaPlus
                  onClick={addNewItem}
                  style={{ cursor: 'pointer' }}
                  aria-label="Добавить новый предмет"
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
        </Container>
        <div className="p-0 m-0 mt-2 text-center">
          <span className="text-uppercase fw-bold">Инвентарь</span>
        </div>
      </Container>
    </>
  );
}

const InventoryCard: React.FC = () => {
  return (
    <>
      <Container className="border rounded">
        <GoldCard />
        <InventoryItemsCard />
      </Container>
    </>
  );
}

export default InventoryCard;
