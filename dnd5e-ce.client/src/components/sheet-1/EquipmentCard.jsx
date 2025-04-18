import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';
import Table from 'react-bootstrap/Table';
import Collapse from 'react-bootstrap/Collapse';

import { useState, Fragment } from 'react';

import { FaCog, FaTrash, FaLock, FaLockOpen, FaPlus, FaTimes, FaWeightHanging } from 'react-icons/fa';

function EditableRow({
  eItem,
  isEditMode,
  onEquipmentItemChange
}) {

  const [name, setName] = useState(eItem.name);
  const handleNameChange = (event) => {
    const newName = event.target.value;
    setName(newName);
    onEquipmentItemChange({ "name": newName })
  }

  const [amount, setAmount] = useState(eItem.amount);
  const handleAmountChange = (event) => {
    const newValue = event.target.value;
    setAmount(newValue);
    onEquipmentItemChange({ amount: newValue });
  };

  const [weight, setWeight] = useState(eItem.weight);
  const handleWeightChange = (event) => {
    const newValue = parseFloat(event.target.value) || 0;
    setWeight(newValue);
    onEquipmentItemChange({ weight: newValue });
  };

  const [prop, setProp] = useState(eItem.prop);
  const handlePropChange = (event) => {
    const newValue = event.target.value;
    setProp(newValue);
    onEquipmentItemChange({ prop: newValue });
  };

  const [mods, setMods] = useState(eItem.mods);
  const handleModsChange = (event) => {
    const newValue = event.target.value;
    setMods(newValue);
    onEquipmentItemChange({ mods: newValue });
  };

  const [isIncluded, setIsIncluded] = useState(eItem.isIncluded);
  const handleIsIncludedChange = (event) => {
    const newValue = event.target.checked;
    setIsIncluded(newValue);
    onEquipmentItemChange({ isIncluded: newValue });
  };

  const [isEquipped, setIsEquipped] = useState(eItem.isEquipped);
  const handleIsEquippedChange = (event) => {
    const newValue = event.target.checked;
    setIsEquipped(newValue);
    onEquipmentItemChange({ isEquipped: newValue });
  };

  const [isUsedAsResource, setIsUsedAsResource] = useState(eItem.isUsedAsResource);
  const handleIsUsedAsResourceChange = (event) => {
    const newValue = event.target.checked;
    setIsUsedAsResource(newValue);
    onEquipmentItemChange({ isUsedAsResource: newValue });
  };

  const [isHasAnAttack, setIsHasAnAttack] = useState(eItem.isHasAnAttack);
  const handleIsHasAnAttackChange = (event) => {
    const newValue = event.target.checked;
    setIsHasAnAttack(newValue);
    onEquipmentItemChange({ isHasAnAttack: newValue });
  };

  const [isExpanded, setExpanded] = useState(false);
  const toggleExpand = () => setExpanded(prev => !prev);
  
  return (
    <Fragment>
      <Row>
        <Col md={1}>
          <Form.Check
            id={`eItem-${eItem.id}-isIncludedCheck`}
            className="m-0 p-0"
            value={isIncluded}
            onChange={handleIsIncludedChange}
          />
        </Col>
        <Col md={1}>
          <Form.Control
            type="text"
            id={`eItem-${eItem.id}-amountEdit`}
            className="m-0 p-0"
            value={amount}
            onChange={handleAmountChange}
          />
        </Col>
        <Col md={"auto"}>
          <Form.Control
            type="text"
            id={`eItem-${eItem.id}-nameEdit`}
            className="m-0 p-0"
            value={name}
            onChange={handleNameChange}
          />
        </Col>
        <Col md={1}>
          <Form.Control
            type="text"
            id={`eItem-${eItem.id}-weightEdit`}
            className="m-0 p-0"
            value={weight}
            onChange={handleWeightChange}
          />
        </Col>
        {isEditMode ? <Col md={1}><FaCog onClick={() => toggleExpand()} /></Col> : null}
      </Row>
      <Collapse in={isExpanded}>
        <Container>
          <Form.Group
            controlId={`eItem-${eItem.id}-propEdit`}
            className="d-flex align-items-center"
          >
            <Form.Label>Свойства</Form.Label>
            <Form.Control
              type="text"
              value={prop}
              className="flex-grow-1"
              onChange={handlePropChange}
            />
          </Form.Group>
          <Form.Group
            controlId={`eItem-${eItem.id}-modsEdit`}
            className="d-flex align-items-center"
          >
            <Form.Label>Дополнительно</Form.Label>
            <Form.Control
              type="text"
              value={mods}
              className="flex-grow-1"
              onChange={handleModsChange}
            />
          </Form.Group>
        </Container>
      </Collapse>
    </Fragment>
  );
}

function EquipmentCard({
  character,
  onEquipmentChange,
  onEquipmentItemChange
}) {

  const [isEditMode, setEditMode] = useState(true); // false -> deletionMode
  const toggleEditMode = () => setEditMode(prev => !prev);

  const [equipment, setEquipment] = useState(character.sheet1.equipment);
  const addEquipment = () => {
    const idArray = equipment.map(t => t.id);
    //console.log(idArray);
    const maxId = idArray.length ? Math.max(...idArray) : 0;
    const newTool = {
      id: maxId + 1,
      isIncluded: true,
      amount: 1,
      name: "ItemName",
      weight: 0,
      isEquipped: true,
      isUsedAsResource: false,
      isHasAnAttack: false,
      prop: "",
      mods: ""
    }
    const newEquipmentArray = [...equipment, newTool]
    setEquipment(newEquipmentArray);
    onEquipmentChange(newEquipmentArray);
  }

  const deleteEquipmentById = (id) => {
    const newEquipment = equipment.filter(e => e.id !== id);
    //console.log(newTools);
    setEquipment(newEquipment);
    onEquipmentChange(newEquipment);
  };



  const equipmentComponents = equipment.map(e => (
    <Fragment key={e.id}>
      <EditableRow
        character={character}
        equipment={e}
        isEditMode={isEditMode}
        onEquipmentItemChange={onEquipmentItemChange} />
      {!isEditMode ? <FaTrash id={e.id} onClick={() => deleteEquipmentById(e.id)} />: null}
    </Fragment>
  ));

  return (
    <>
      <Container className="border-1 border-rounded-1">
        <Container fluid={true} className="p-0 m-0">
          <Row className="fw-bold">
            <Col md={1}></Col>
            <Col md={1}>{FaTimes}</Col>
            <Col md={"auto"}>Имя</Col>
            <Col md={1}>{FaWeightHanging}</Col>
            {isEditMode ? <Col md={1}></Col> : null}
          </Row>
          <Row>
            {equipmentComponents}
          </Row>
          {isEditMode ? <FaLock onClick={() => toggleEditMode()} /> : <FaLockOpen onClick={() => toggleEditMode()} />}
          {isEditMode ? <FaPlus onClick={() => addEquipment()} /> : null}
        </Container>
        <div className="p-0 m-0 mt-2 text-center">
          <span className="text-uppercase fw-bold">Экипировка</span>
        </div>
      </Container>
    </>
    
  );
}

export default EquipmentCard;