import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';
import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import ClassResource from './ClassResourceCard';
import OtherResourceCard from './OtherResourceCard';
import { FaCog, FaLock, FaLockOpen, FaPlus } from 'react-icons/fa';

import { v4 as uuidv4 } from 'uuid';
import { useState } from 'react';
import { useAppDispatch, useAppSelector } from '../../hooks';
import { OtherResource, OtherResourcePropsType, ResourceResetType } from '../../types/state';
import { addCharacterOtherResource, deleteCharacterOtherResource, updateCharacterClassResourceCurrent, updateCharacterClassResourceName, updateCharacterClassResourceResetOn, updateCharacterClassResourceTotal, updateCharacterClassResourceUsePb } from '../../store/sheet1Slice';
import { selectOtherResources } from '../../store/selectors/sheet1Selectors';

const CharacterResourceComponent: React.FC = () => {
  const dispatch = useAppDispatch();

  const [isEditMode, setIsEditMode] = useState(true);
  const toggleIsEditMode = () => setIsEditMode(!isEditMode);

  const addNewResource = () => {
    const newResource: OtherResource = {
      id: uuidv4(),
      name: 'Новый ресурс',
      total: 0,
      current: 0,
      usePb: false,
      resetOn: 'longRest',
    };
    dispatch(addCharacterOtherResource(newResource));
  };

  const resources = useAppSelector(selectOtherResources);
  const resourcesComponents = resources.map((resource) =>
    <Col key={resource.id} md={6}>
      <OtherResourceCard
        resource={resource}
        isEditMode={isEditMode}
        onDelete={() => dispatch(deleteCharacterOtherResource(resource.id))}
      />
    </Col>
  );

  return (
    <Container>
      <Row className="d-flex flex-row">
        <Col md={6}>
          <ClassResource />
        </Col>
        {resourcesComponents}
      </Row>
      <Row className="mt-3">
        <Col className="d-flex justify-content-center gap-3">
          {isEditMode ? (
            <>
              <FaPlus
                onClick={addNewResource}
                style={{ cursor: 'pointer' }}
                aria-label="Добавить новый ресурс"
              />
              <FaLock
                onClick={toggleIsEditMode}
                style={{ cursor: 'pointer' }}
                aria-label="Переключить в режим удаления"
              />
            </>
          ) : (
            <FaLockOpen
              onClick={toggleIsEditMode}
              style={{ cursor: 'pointer' }}
              aria-label="Переключить в режим редактирования"
            />
          )}
        </Col>
      </Row>
    </Container>
  );
}

export default CharacterResourceComponent;