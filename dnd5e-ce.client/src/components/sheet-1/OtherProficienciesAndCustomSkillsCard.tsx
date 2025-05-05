import Card from 'react-bootstrap/Card';
import Col from 'react-bootstrap/Col';
import Collapse from 'react-bootstrap/Collapse';
import Container from 'react-bootstrap/Container';
import Form from 'react-bootstrap/Form';
import Row from 'react-bootstrap/Row';

import { FaCog, FaLock, FaLockOpen, FaPlus, FaTrash } from 'react-icons/fa';

import { Fragment, useEffect, useState } from 'react';
import { v4 as uuidv4 } from 'uuid';
import { useAppDispatch, useAppSelector } from '../../hooks';
import { selectOtherTools } from '../../store/selectors/sheet1Selectors';
import { addOtherTool, deleteOtherTool, updateOtherTool } from '../../store/sheet1Slice';
import { OtherProficienciesAndCustomSkillsCardEditableRowPropsType, OtherTool, OtherToolType } from '../../types/state';



const EditableRow: React.FC<OtherProficienciesAndCustomSkillsCardEditableRowPropsType> = ({
  tool,
  isEditMode,
  onDelete,
}) => {
  const dispatch = useAppDispatch();

  const [name, setName] = useState(tool.name);
  const [type, setType] = useState<OtherToolType>(tool.type);

  const [isExpanded, setExpanded] = useState(false);

  // Сворачиваем карточку при переходе в режим удаления
  useEffect(() => {
    if (!isEditMode) {
      setExpanded(false);
    }
  }, [isEditMode]);

  const toggleExpand = () => setExpanded((prev) => !prev);

  const handleNameChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newName = event.target.value;
    setName(newName);
    dispatch(updateOtherTool({ ...tool, name: newName }));
  };

  const handleTypeChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const newType = event.target.value as OtherToolType;
    setType(newType);
    dispatch(updateOtherTool({ ...tool, type: newType }));
  };

  const displayType = (type: OtherToolType) => {
    switch (type.toLowerCase()) {
      case 'language':
        return 'Язык';
      case 'weapon':
        return 'Оружие';
      case 'armor':
        return 'Броня';
      case 'other':
        return 'Другое';
      default:
        return 'Ошибка';
    }
  };

  return (
    <Fragment>
      <Row className="align-items-center py-1">
        <Col md={4}>{displayType(type)}</Col>
        <Col md={6} style={{ whiteSpace: 'normal', wordWrap: 'break-word' }}>
          {name}
        </Col>
        <Col md={1} className="text-center">
          {isEditMode ? (
            <FaCog
              onClick={toggleExpand}
              style={{ cursor: 'pointer' }}
              aria-label="Редактировать владение"
            />
          ) : (
            <FaTrash
              onClick={onDelete}
              style={{ cursor: 'pointer', color: 'red' }}
              aria-label="Удалить владение"
            />
          )}
        </Col>
      </Row>
      <Collapse in={isExpanded}>
        <div className="p-3">
          <Form.Group controlId={`otherTool-${tool.id}-typeEdit`} className="mb-2">
            <Form.Label>Тип</Form.Label>
            <Form.Select value={type} onChange={handleTypeChange}>
              <option value="language">Язык</option>
              <option value="weapon">Оружие</option>
              <option value="armor">Броня</option>
              <option value="other">Другое</option>
            </Form.Select>
          </Form.Group>
          <Form.Group controlId={`otherTool-${tool.id}-nameEdit`} className="mb-2">
            <Form.Label>Имя</Form.Label>
            <Form.Control
              type="text"
              value={name}
              onChange={handleNameChange}
              placeholder="Например, Эльфийский язык"
            />
          </Form.Group>
        </div>
      </Collapse>
    </Fragment>
  );
};

const OtherProficienciesAndCustomSkillsCard: React.FC = () => {
  const dispatch = useAppDispatch();
  const otherTools = useAppSelector(selectOtherTools);

  const [isEditMode, setEditMode] = useState(true); // true -> edit mode, false -> deletion mode
  const toggleEditMode = () => setEditMode((prev) => !prev);

  const addNewTool = () => {
    const newTool: OtherTool = {
      id: uuidv4(),
      name: 'Новое владение',
      type: 'other',
    };
    dispatch(addOtherTool(newTool));
  };

  const deleteToolById = (id: string) => () => dispatch(deleteOtherTool(id));

  return (
    <Card className="p-0 m-0 border-0 w-100">
      <Card.Body className="p-3">
        <Container fluid className="p-3 border rounded">
          <Row className="fw-bold mb-2">
            <Col md={5}>Тип</Col>
            <Col md={6}>Владение</Col>
            <Col md={1} className="text-center"></Col>
          </Row>
          {otherTools.length > 0 ? (
            otherTools.map((tool) => (
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
                    className="icon-action"
                    aria-label="Добавить новое владение"
                  />
                  <FaLock
                    onClick={toggleEditMode}
                    className="icon-action"
                    aria-label="Переключить в режим удаления"
                  />
                </>
              ) : (
                <FaLockOpen
                  onClick={toggleEditMode}
                  className="icon-action"
                  aria-label="Переключить в режим редактирования"
                />
              )}
            </Col>
          </Row>
        </Container>
        <div className="p-0 m-0 mt-2 text-center">
          <span className="text-uppercase fw-bold">Прочие владения и умения</span>
        </div>
      </Card.Body>
    </Card>
  );
};

export default OtherProficienciesAndCustomSkillsCard;