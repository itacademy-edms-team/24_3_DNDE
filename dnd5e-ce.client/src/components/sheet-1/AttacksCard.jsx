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
  attack,
  isEditMode,
  onAttackChange
}) {

  const [isExpanded, setExpanded] = useState(false);
  const toggleExpand = () => setExpanded(prev => !prev);

  const [attackName, setAttackName] = useState(attack.name);
  const handleAttackNameChange = (event) => {
    const newValue = event.target.value;
    onAttackChange(attack.id, { name: newValue });
    setAttackName(newValue);
  }

  const [isAtkIncluded, setIsAtkIncluded] = useState(attack.atk.isIncluded);
  const handleIsAtkIncludedChange = (event) => {
    const newValue = event.target.checked;
    onAttackChange(attack.id, { atk: { ...attack.atk, isIncluded: newValue } });
    setIsAtkIncluded(newValue);
  };

  const [atkBondAbility, setAtkBondAbility] = useState(attack.atk.bondAbility);
  const handleAtkBondAbilityChange = (event) => {
    const newValue = event.target.value;
    onAttackChange(attack.id, { atk: { ...attack.atk, bondAbility: newValue } });
    setAtkBondAbility(newValue);
  };

  const [atkBonus, setAtkBonus] = useState(attack.atk.bonus);
  const handleAtkBonusChange = (event) => {
    const newValue = Number(event.target.value);
    onAttackChange(attack.id, { atk: { ...attack.atk, bonus: newValue } });
    setAtkBonus(newValue);
  };

  const [IsAtkProficient, setAtkIsProficient] = useState(attack.atk.isProficient);
  const handleIsAtkProficientChange = (event) => {
    const newValue = event.target.checked;
    onAttackChange(attack.id, { atk: { ...attack.atk, isProficient: newValue } });
    setAtkIsProficient(newValue);
  };

  const [atkRange, setAtkRange] = useState(attack.atk.range);
  const handleAtkRangeChange = (event) => {
    const newValue = event.target.value;
    onAttackChange(attack.id, { atk: { ...attack.atk, range: newValue } });
    setAtkRange(newValue);
  };

  const [atkMagicBonus, setAtkMagicBonus] = useState(attack.atk.magicBonus);
  const handleAtkMagicBonusChange = (event) => {
    const newValue = Number(event.target.value);
    onAttackChange(attack.id, { atk: { ...attack.atk, magicBonus: newValue } });
    setAtkMagicBonus(newValue);
  };

  const [atkCritRange, setAtkCritRange] = useState(attack.atk.critRange);
  const handleAtkCritRangeChange = (event) => {
    const newValue = event.target.value;
    onAttackChange(attack.id, { atk: { ...attack.atk, critRange: newValue } });
    setAtkCritRange(newValue);
  };

  return (
    <Fragment>
      <Row>
        <Col md={3}>{attackName}</Col>
        <Col md={3}>{0}</Col>
        <Col md={4}></Col>
        {isEditMode ? <Col md={1}><FaCog onClick={() => toggleExpand()} /></Col> : null}
      </Row>
      <Collapse in={isExpanded}>
        <Container>
          <Container className="p-0 m-0">
            <Form.Group
              controlId={`attack-${attack.id}-editName`}
              className="m-0 p-0 d-flex gap-1"
            >
              <Form.Label className="p-0 m-0 fw-bold">Имя</Form.Label>
              <Form.Control
                type="text"
                className="p-0 m-0"
                value={attackName}
                onChange={handleAttackNameChange}
              />
            </Form.Group>
          </Container>
          <Row className="pt-1">
            <Col md={1} className="m-0">
              <Form.Check
                id={`attack-${attack.id}-isAtkIncluded`}
                inline={true}
                value={isAtkIncluded}
                onChange={handleIsAtkIncludedChange}
              />
            </Col>
            <Col md={11} className="m-0 d-flex flex-column gap-1">
              <Container className="p-0 m-0 d-flex gap-1">
                <Form.Group
                  controlId={`attack-${attack.id}-editAtkBondAbility`}
                  className="m-0 p-0 d-flex gap-1"
                >
                  <Form.Label className="p-0 m-0 fw-bold">Атака:</Form.Label>
                  <Form.Select
                    className="m-0 p-0 w-auto"
                    value={atkBondAbility}
                    onChange={handleAtkBondAbilityChange}
                  >
                    <option value="strength">МАГ</option>
                    <option value="dexterity">ЛОВ</option>
                    <option value="constitution">ТЕЛ</option>
                    <option value="intelligence">ИНТ</option>
                    <option value="wisdom">МУД</option>
                    <option value="charisma">ХАР</option>
                    <option value="-">-</option>
                  </Form.Select>
                </Form.Group>
                <span className="fw-bold">+</span>
                <Form.Group
                  controlId={`attack-${attack.id}-editAtkBonus`}
                  className="m-0 p-0 d-flex"
                  style={{ width: "2rem" }}
                >
                  <Form.Control
                    type="text"
                    className="m-0 p-0"
                    value={atkBonus}
                    onChange={handleAtkBonusChange}
                  />
                </Form.Group>
                <span className="fw-bold">+</span>
                <Form.Group
                  controlId={`attack-${attack.id}-isAtkProficient`}
                  className="m-0 p-0 d-flex"
                >
                  <Form.Check
                    inline={true}
                    className="p-0 m-0 px-1"
                    value={IsAtkProficient}
                    onChange={handleIsAtkProficientChange}
                  />
                  <Form.Label className="m-0 p-0"><span className="m-0 p-0 fw-bold">Владение</span></Form.Label>
                </Form.Group>
              </Container>
              <Container className="p-0 m-0 d-flex gap-1">
                <Form.Group
                  controlId={`attack-${attack.id}-editRange`}
                  className="m-0 p-0 d-flex gap-1"
                >
                  <Form.Label className="m-0 p-0"><span className="m-0 p-0 fw-bold">Дальность:</span></Form.Label>
                  <Form.Control
                    type="text"
                    className="m-0 p-0"
                    value={atkRange}
                    onChange={handleAtkRangeChange}
                  />
                </Form.Group>
              </Container>
              <Container className="p-0 m-0 d-flex gap-1">
                <Form.Group
                  controlId={`attack-${attack.id}-editAtkMagicBonus`}
                  className="m-0 p-0 d-flex gap-1"
                >
                  <Form.Label className="m-0 p-0">
                    <span className="m-0 p-0 fw-bold text-nowrap">Маг бонус:</span>
                  </Form.Label>
                  <Form.Control
                    type="text"
                    className="m-0 p-0"
                    value={atkMagicBonus}
                    onChange={handleAtkMagicBonusChange}
                  />
                </Form.Group>
              </Container>
              <Container className="p-0 m-0 d-flex gap-1">
                <Form.Group
                  controlId={`attack-${attack.id}-editAtkCritRange`}
                  className="m-0 p-0 d-flex gap-1"
                >
                  <Form.Label className="m-0 p-0">
                    <span className="m-0 p-0 fw-bold text-nowrap">Крит дальность:</span>
                  </Form.Label>
                  <Form.Control
                    type="text"
                    className="m-0 p-0"
                    value={atkCritRange}
                    onChange={handleAtkCritRangeChange}
                  />
                </Form.Group>
              </Container>
            </Col>
          </Row>
        </Container>
      </Collapse>
    </Fragment>
  );
}

function AttacksCard({
  character,
  onAttacksChange,
  onAttackChange
}) {

  const [isEditMode, setEditMode] = useState(true); // false -> deletionMode
  const toggleEditMode = () => setEditMode(prev => !prev);

  const [attacks, setAttacks] = useState(character.sheet1.attacks);
  const addAttack = () => {
    const idArray = attacks.map(t => t.id);
    //console.log(idArray);
    const maxId = idArray.length ? Math.max(...idArray) : 0;
    const newAttack = {
      id: maxId + 1,
      name: "Attack0",
      attack: {
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
        critDice: "1d6"
      },
      damage2: {
        isIncluded: false,
        dice: "1d6",
        bondAbility: "strength", // strength, dexterity, constitution, intelligence, wisdom, charisma, spell, "-"
        bonus: 0,
        type: "",
        critDice: "1d6"
      },
      savingThrow: {
        isIncluded: false,
        bondAbility: "strength",
        dificultyClass: "strength"
      },
      saveEffect: "",
      description: ""
    }
    const newToolArray = [...attacks, newAttack]
    setAttacks(newToolArray);
    onAttacksChange(newToolArray);
  }

  const deleteAttackById = (id) => {
    const newTools = attacks.filter(tool => tool.id !== id);
    //console.log(newTools);
    setAttacks(newTools);
    onAttacksChange(newTools);
  };

  const attackComponents = attacks.map(a => (
    <Fragment key={a.id}>
      <EditableRow
        character={character}
        attack={a}
        isEditMode={isEditMode}
        onAttackChange={onAttackChange} />
      {!isEditMode ? <FaTrash id={a.id} onClick={() => deleteAttackById(a.id)} />: null}
    </Fragment>
  ));

  return (
    <>
      <Container className="border-1 border-rounded-1">
        <Container fluid={true} className="p-0 m-0">
          <Row className="fw-bold">
            <Col md={3}>Имя</Col>
            <Col md={3}>Атака</Col>
            <Col md={4}>Урон/Тип</Col>
            {isEditMode ? <Col md={1}></Col> : null}
          </Row>
          <Row>
            {attackComponents}
          </Row>
          {isEditMode ? <FaLock onClick={() => toggleEditMode()} /> : <FaLockOpen onClick={() => toggleEditMode()} />}
          {isEditMode ? <FaPlus onClick={() => addAttack()} /> : null}
        </Container>
      </Container>
    </>
    
  );
}

export default AttacksCard;