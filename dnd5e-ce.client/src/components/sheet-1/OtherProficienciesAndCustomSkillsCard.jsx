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
  const [type, setType] = useState(tool.type);

  const [isExpanded, setExpanded] = useState(false);
  const toggleExpand = () => setExpanded(prev => !prev);

  const handleNameChange = (event) => {
    const newName = event.target.value;
    setName(newName);
    onToolChange(tool.id, { name: newName });
  }

  const handleTypeChange = (event) => {
    const selectedType = event.target.value;
    setType(selectedType);
    onToolChange(tool.id, { type: selectedType });
  }

  function displayType(type) {
    switch (type.toLowerCase()) {
      case "language":
        return "Язык";
      case "weapon":
        return "Оружие";
      case "armor":
        return "Броня";
      case "other":
        return "Другое";
      default:
        return "Error";
    }
  }

  return (
    <Fragment>
      <Row>
        <Col md={3}>{displayType(type)}</Col>
        <Col md={8}>{name}</Col>
        {isEditMode ? <Col md={1}><FaCog onClick={() => toggleExpand()} /></Col> : null}
      </Row>
      <Collapse in={isExpanded}>
        <Container>
          <Form.Group controlId={`otherTool-${tool.id}-typeEdit`} >
            <Form.Label>Тип</Form.Label>
            <Form.Select
              value={type}
              onChange={handleTypeChange}
            >
              <option value="language">Язык</option>
              <option value="weapon">Оружие</option>
              <option value="armor">Броня</option>
              <option value="other">Другое</option>
            </Form.Select>
          </Form.Group>
          <Form.Group controlId={`otherTool-${tool.id}-nameEdit`}>
            <Form.Label>Имя</Form.Label>
            <Form.Control
              type="text"
              value={name}
              onChange={handleNameChange}
            />
          </Form.Group>
        </Container>
      </Collapse>
    </Fragment>
  );
}

function OtherProficienciesAndCustomSkillsCard({
  character,
  onToolsChange,
  onToolChange
}) {

  const [isEditMode, setEditMode] = useState(true); // false -> deletionMode
  const toggleEditMode = () => setEditMode(prev => !prev);

  const [tools, setTools] = useState(character.sheet1.otherTools);
  const addTool = () => {
    const idArray = tools.map(t => t.id);
    //console.log(idArray);
    const maxId = idArray.length ? Math.max(...idArray) : 0;
    const newTool = {
      id: maxId + 1,
      name: "Unnamed",
      type: "language",
    }
    const newToolArray = [...tools, newTool]
    setTools(newToolArray);
    onToolsChange(newToolArray);
  }

  const deleteToolById = (id) => {
    const newTools = tools.filter(tool => tool.id !== id);
    //console.log(newTools);
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
            <Col md={4}>Тип</Col>
            <Col md={7}>Владение</Col>
            {isEditMode ? <Col md={1}></Col> : null}
          </Row>
          <Row>
            {toolComponents}
          </Row>
          {isEditMode ? <FaLock onClick={() => toggleEditMode()} /> : <FaLockOpen onClick={() => toggleEditMode()} />}
          {isEditMode ? <FaPlus onClick={() => addTool()} /> : null}
        </Container>
        <div className="p-0 m-0 mt-2 text-center">
          <span className="text-uppercase fw-bold">Прочие владения и умения</span>
        </div>
      </Container>
    </>
    
  );
}

export default OtherProficienciesAndCustomSkillsCard;