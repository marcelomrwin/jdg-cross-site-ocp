package cache

import (
	"EmployeeGolangApp/models"
	"bytes"
	"crypto/tls"
	"encoding/json"
	"errors"
	"fmt"
	"github.com/caitlinelfring/go-env-default"
	"net/http"
	"strconv"
)

type Configuration struct {
	Protocol string
	Host     string
	Port     int
	Cache    string
	User     string
	Password string
}

type DataGridRestClient struct {
	cacheConfiguration Configuration
}

func NewDataGridRestClient() DataGridRestClient {
	return DataGridRestClient{GetCacheConfiguration()}
}

func buildHttpClient() *http.Client {
	tr := &http.Transport{
		TLSClientConfig: &tls.Config{InsecureSkipVerify: true},
	}
	client := &http.Client{Transport: tr}
	return client
}

func GetCacheConfiguration() Configuration {
	cacheConfig := Configuration{
		Protocol: env.GetDefault("CACHE_PROTOCOL", "https"),
		Host:     env.GetDefault("CACHE_HOST", "127.0.0.1"),
		Port:     env.GetIntDefault("CACHE_PORT", 11222),
		Cache:    env.GetDefault("CACHE_NAME", "employees"),
		User:     env.GetDefault("CACHE_USER", "admin"),
		Password: env.GetDefault("CACHE_PASSWD", "password"),
	}
	return cacheConfig
}

func (dg *DataGridRestClient) GetAllKeysFromCache() ([]string, error) {
	//GET /rest/v2/caches/{cacheName}?action=keys
	req, err := http.NewRequest("GET", dg.BaseUrl()+"/caches/"+dg.cacheConfiguration.Cache+"?action=keys", nil)
	if err != nil {
		return nil, err
	}

	client := buildHttpClient()
	req.Header.Set("Accept", "application/json")

	resp, err := client.Do(req)
	if err != nil {
		return nil, err
	}
	defer resp.Body.Close()
	var keys []string
	err = json.NewDecoder(resp.Body).Decode(&keys)
	if err != nil {
		return nil, err
	}

	return keys, nil
}

func (dg *DataGridRestClient) BaseUrl() string {
	return dg.cacheConfiguration.Protocol + "://" + dg.cacheConfiguration.Host + ":" + strconv.Itoa(dg.cacheConfiguration.Port) + "/rest/v2"
}

func (dg *DataGridRestClient) KeyExistsInCache(uuid string) (bool, error) {

	req, err := http.NewRequest("HEAD", dg.BaseUrl()+"/caches/"+dg.cacheConfiguration.Cache+"/"+uuid, nil)
	if err != nil {
		return false, err
	}

	client := buildHttpClient()
	resp, err := client.Do(req)
	if err != nil {
		return false, err
	}

	exists := resp.StatusCode == http.StatusOK

	return exists, nil
}

func (dg *DataGridRestClient) GetEmployeeFromCache(uuid string) (*models.EmployeeDTO, error) {
	//GET /rest/v2/caches/{cacheName}/{cacheKey}
	req, err := http.NewRequest("GET", dg.BaseUrl()+"/caches/"+dg.cacheConfiguration.Cache+"/"+uuid, nil)
	if err != nil {
		return nil, err
	}

	req.Header.Set("Accept", "application/json")
	client := buildHttpClient()
	resp, err := client.Do(req)
	if err != nil {
		return nil, err
	}

	defer resp.Body.Close()
	var employeeDto *models.EmployeeDTO
	err = json.NewDecoder(resp.Body).Decode(&employeeDto)
	if err != nil {
		return nil, err
	}

	return employeeDto, nil

}

func (dg *DataGridRestClient) AddToCache(dto *models.EmployeeDTO) error {

	jsonData, err := json.Marshal(dto)
	if err != nil {
		return err
	}

	exists, err := dg.KeyExistsInCache(dto.UUID)
	if err != nil {
		return err
	}

	client := buildHttpClient()

	if exists {
		//update

		//PUT /rest/v2/caches/{cacheName}/{cacheKey}
		req, err := http.NewRequest("PUT", dg.BaseUrl()+"/caches/"+dg.cacheConfiguration.Cache+"/"+dto.UUID, bytes.NewBuffer(jsonData))
		if err != nil {
			return err
		}
		req.Header.Set("Content-Type", "application/json")
		req.Header.Set("Accept", "application/json")

		resp, err := client.Do(req)
		if err != nil {
			return err
		}

		if resp.StatusCode != http.StatusNoContent {
			return errors.New(fmt.Sprintf("request failed with status code %d", resp.StatusCode))
		}

	} else {
		//create
		//POST /rest/v2/caches/{cacheName}/{cacheKey}
		req, err := http.NewRequest("POST", dg.BaseUrl()+"/caches/"+dg.cacheConfiguration.Cache+"/"+dto.UUID, bytes.NewBuffer(jsonData))
		if err != nil {
			return err
		}
		req.Header.Set("Content-Type", "application/json")
		req.Header.Set("Accept", "application/json")

		resp, err := client.Do(req)
		if err != nil {
			return err
		}

		if resp.StatusCode != http.StatusNoContent {
			return errors.New(fmt.Sprintf("request failed with status code %d", resp.StatusCode))
		}
	}

	return nil
}
