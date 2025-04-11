import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';
import Table from 'react-bootstrap/Table';
import Collapse from 'react-bootstrap/Collapse';

import { useState, Fragment } from 'react';

import { FaCog, FaTrash, FaLock, FaLockOpen, FaPlus } from 'react-icons/fa';

function EditableRow({
  character,
  tool,
  isEditMode,
  onToolChange
}) {

  const [name, setName] = useState(tool.name);
  const [proficiencyType, setProficiencyType] = useState(tool.proficiencyType);
  const [bondAbility, setBondAbility] = useState(tool.bondAbility);
  const [mods, setMods] = useState(tool.mods);

  const [isExpanded, setExpanded] = useState(false);
  const toggleExpand = () => setExpanded(prev => !prev);

  const handleNameChange = (event) => {
    const newName = event.target.value;
    setName(newName);
    onToolChange(tool.id, { name: newName });
  }

  const handleProficiencyTypeChange = (event) => {
    const selectedProficiencyType = event.target.value;
    setProficiencyType(selectedProficiencyType);
    onToolChange(tool.id, { proficiencyType: selectedProficiencyType });
  }

  const handleAbilityChange = (event) => {
    const selectedAbility = event.target.value;
    setBondAbility(selectedAbility);
    onToolChange(tool.id, { bondAbility: selectedAbility });
  }

  const handleModsChange = (event) => {
    const newMods = Number(event.target.value);
    setMods(newMods);
    onToolChange(tool.id, { mods: newMods });
  }

  function calcPb(pType) {
    let pb = 1;
    if (pType === "proficient") pb *= 1;
    else if (pType === "expertise") pb *= 2;
    else if (pType === "jackOfAllTrades") pb *= 1.5;
    return pb;
  }

  function calcProficiency() {
    if (bondAbility == "queryAttribute") return "?";

    const abilityBase = character.sheet1.characteristics[bondAbility].base;
    const am = Math.floor((abilityBase) / 2);

    const pb = calcPb(proficiencyType);

    return am + pb + mods;
  }

  function displayAttribute() {
    switch (bondAbility.toLowerCase()) {
      case "strength":
        return "Сила";
      case "dexterity":
        return "Ловкость";
      case "constitution":
        return "Телосложение";
      case "intelligence":
        return "Интеллект";
      case "wisdom":
        return "Мудрость";
      case "charisma":
        return "Харизма";
      case "queryattribute":
        return "Другая"
      default:
        return "Error";
    }
  }

  return (
    <Fragment>
      <Row>
        <Col md={3}>{name}</Col>
        <Col md={4} className="text-center">{calcProficiency()}</Col>
        <Col md={3}>{displayAttribute()}</Col>
        {isEditMode ? <Col><FaCog onClick={() => toggleExpand()} /></Col> : null}
      </Row>
      <Collapse in={isExpanded}>
        <Container>
          <Form.Group controlId={`tool-${tool.id}-nameEdit`}>
            <Form.Label>Имя</Form.Label>
            <Form.Control
              type="text"
              value={name}
              onChange={handleNameChange}
            />
          </Form.Group>
          <Form.Group controlId={`tool-${tool.id}-proficiencyTypeEdit`}>
            <Form.Label>Уровень владения</Form.Label>
            <Form.Select
              value={proficiencyType}
              onChange={handleProficiencyTypeChange}
            >
              <option value="proficient">Владение</option>
              <option value="expertise">Экспертиза</option>
              <option value="jackOfAllTrades">Мастер на все руки</option>
            </Form.Select>
          </Form.Group>
          <Form.Group controlId={`tool-${tool.id}-bondAtributeEdit`} >
            <Form.Label>Характеристика</Form.Label>
            <Form.Select
              value={bondAbility}
              onChange={handleAbilityChange}
            >
              <option value="strength">Сила</option>
              <option value="dexterity">Ловкость</option>
              <option value="constitution">Телосложение</option>
              <option value="intelligence">Интелект</option>
              <option value="wisdom">Мудрость</option>
              <option value="charisma">Харизма</option>
              <option value="queryAttribute">Другая</option>
            </Form.Select>
          </Form.Group>
          <Form.Group controlId={`tool-${tool.id}-bondAtributeEdit`} >
            <Form.Label>Бонус</Form.Label>
            <Form.Control
              type="number"
              value={mods}
              onChange={handleModsChange}
            />
          </Form.Group>
        </Container>
      </Collapse>
    </Fragment>
  );
}

function ToolProficienciesAndCustomSkillsCard({
  character,
  onToolsChange,
  onToolChange
}) {

  const [isEditMode, setEditMode] = useState(true); // false -> deletionMode
  const toggleEditMode = () => setEditMode(prev => !prev);

  const [tools, setTools] = useState(character.sheet1.tools);
  const addTool = () => {
    const idArray = tools.map(t => t.id);
    //console.log(idArray);
    const maxId = idArray.length ? Math.max(...idArray) : 0;
    const newTool = {
      id: maxId + 1,
      name: "Unnamed",
      proficiencyType: "proficient", // proficient | expertise | jackOfAllTrades
      bondAbility: "strength",
      mods: 0
    }
    const newToolArray = [...tools, newTool]
    setTools(newToolArray);
    onToolsChange(newToolArray);
  }

  const deleteToolById = (id) => {
    const newTools = tools.filter(tool => tool.id !== id);
    console.log(newTools);
    setTools(newTools);
    onToolsChange(newTools);
  };



  const toolComponents = tools.map(t => (
    <Fragment key={t.id}>
      <EditableRow
        character={character}
        tool={t}
        isEditMode={isEditMode}
        onToolChange={onToolChange} />
      {!isEditMode ? <FaTrash id={t.id} onClick={() => deleteToolById(t.id)} />: null}
    </Fragment>
  ));

  return (
    <>
      <Container className="border-1 border-rounded-1">
        <Container fluid={true} className="p-0 m-0">
          <Row className="fw-bold">
            <Col md={3}>Имя</Col>
            <Col md={4}>Владение</Col>
            <Col md={3}>Хар-ка</Col>
            {isEditMode ? <Col></Col> : null}
          </Row>
          <Row>
            {toolComponents}
          </Row>
          {isEditMode ? <FaLock onClick={() => toggleEditMode()} /> : <FaLockOpen onClick={() => toggleEditMode()} />}
          {isEditMode ? <FaPlus onClick={() => addTool()} /> : null}
        </Container>
        <div className="p-0 m-0 mt-2 text-center">
          <span className="text-uppercase fw-bold">Владение снаряжением и Умения</span>
        </div>
      </Container>
    </>
    
  );
}

export default ToolProficienciesAndCustomSkillsCard;